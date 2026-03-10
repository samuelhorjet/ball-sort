using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using BallSort.Core;

namespace BallSort.Game
{
    /// <summary>
    /// Orchestrates the full puzzle lifecycle:
    ///   open_session → init_puzzle → VRF wait → ER setup → start_puzzle
    ///   → apply_move / apply_undo → undelegate → finalize_puzzle → close_session
    ///
    /// Updated for Versioned Transactions (v0) and Deep Linking.
    /// </summary>
    public class PuzzleOrchestrator : MonoBehaviour
    {
        public static PuzzleOrchestrator Instance { get; private set; }

        // ── Cached account data ───────────────────────────────────────────────
        public Blockchain.PlayerAuth CurrentPlayerAuth { get; private set; }
        public Blockchain.PuzzleBoard CurrentPuzzleBoard { get; private set; }
        public Blockchain.PuzzleStats CurrentPuzzleStats { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Player auth ───────────────────────────────────────────────────────

        public async Task EnsurePlayerAuthAsync()
        {
            var wallet = Core.WalletManager.Instance;
            var playerKey = wallet.PublicKey;
            var (playerAuthPDA, _) = Blockchain.PDAHelper.GetPlayerAuthPDA(playerKey);
            var (playerProfilePDA, _) = Blockchain.PDAHelper.GetPlayerProfilePDA(playerKey);
            var (gameConfigPDA, _) = Blockchain.PDAHelper.GetGameConfigPDA();

            var data = await SolanaManager.Instance.GetAccountDataAsync(playerAuthPDA);

            if (data == null)
            {
                Debug.Log("[Orch] Creating player auth...");
                var ix = Blockchain.ProgramClient.BuildCreatePlayerAuthIx(
                    playerKey, playerAuthPDA, gameConfigPDA, playerProfilePDA);
                var tx = await Blockchain.TxBuilder.BuildAsync(ix, feePayer: playerKey);
                var sig = await wallet.SignAndSendAsync(tx);
                await SolanaManager.Instance.ConfirmTransactionAsync(sig);
                data = await SolanaManager.Instance.GetAccountDataAsync(playerAuthPDA);
            }

            CurrentPlayerAuth = Blockchain.AccountDeserializer.DeserializePlayerAuth(data);
            Debug.Log($"[Orch] PlayerAuth loaded. Nonce={CurrentPlayerAuth.PuzzlesStartedNonce}");

            // ── Stale puzzle cleanup ──────────────────────────────────────────
            // If a previous session left has_active_puzzle=true on-chain,
            // initPuzzle will throw PuzzleAlreadyActive (0x1776).
            // Abandon it now so the player can start fresh.
            if (CurrentPlayerAuth.HasActivePuzzle)
            {
                Debug.LogWarning("[Orch] Stale active puzzle detected — abandoning before starting new one.");
                try
                {
                    var staleIx = Blockchain.ProgramClient.BuildAbandonPuzzleIx(
                        playerKey, playerAuthPDA, gameConfigPDA);
                    var staleTx = await Blockchain.TxBuilder.BuildAsync(staleIx, feePayer: playerKey);
                    var staleSig = await wallet.SignAndSendAsync(staleTx);
                    await SolanaManager.Instance.ConfirmTransactionAsync(staleSig);

                    // Refresh so HasActivePuzzle = false and nonce is up-to-date
                    data = await SolanaManager.Instance.GetAccountDataAsync(playerAuthPDA);
                    CurrentPlayerAuth = Blockchain.AccountDeserializer.DeserializePlayerAuth(data);
                    Debug.Log("[Orch] Stale puzzle abandoned. Ready to start fresh.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Orch] Could not abandon stale puzzle: {e.Message}. Proceeding anyway.");
                }
            }
        }

        // ── Full puzzle start flow ────────────────────────────────────────────

        public async Task StartNewPuzzleAsync(byte numTubes, byte ballsPerTube)
        {
            var gstate = GameStateManager.Instance;
            var wallet = Core.WalletManager.Instance;
            var playerKey = wallet.PublicKey;

            gstate.GoToGameLoading();

            try
            {
                // ── Step 1: Open session ─────────────────────────────────────
                gstate.SetLoadingStep(LoadingStep.OpeningSession, "Opening session key...");
                await Core.SessionKeyManager.Instance.OpenSessionAsync();

                // ── Derive PDAs ───────────────────────────────────────────────
                var (playerAuthPDA, _) = Blockchain.PDAHelper.GetPlayerAuthPDA(playerKey);
                var (gameConfigPDA, _) = Blockchain.PDAHelper.GetGameConfigPDA();
                var nonce = CurrentPlayerAuth.PuzzlesStartedNonce;
                var (boardPDA, _) = Blockchain.PDAHelper.GetPuzzleBoardPDA(playerAuthPDA, nonce);
                var (statsPDA, _) = Blockchain.PDAHelper.GetPuzzleStatsPDA(playerAuthPDA, nonce);

                // Session is now open; use its key for all remaining transactions.
                // This matches the TypeScript test where sessionKeypair signs everything
                // from initPuzzle onward — zero wallet popups during gameplay.
                var session = Core.SessionKeyManager.Instance;

                // ── Step 2: init_puzzle ───────────────────────────────────────
                // Signed by SESSION KEY (matches test: tx.sign(sessionKeypair))
                gstate.SetLoadingStep(LoadingStep.InitializingPuzzle, "Requesting VRF randomness...");
                // Use player-selected difficulty (1=Easy, 2=Medium, 3=Hard)
                // Difficulty controls how thoroughly VRF randomness shuffles the balls.
                // Hardcoding 0 = no shuffle → all same-color balls per tube.
                byte difficulty = Core.GameStateManager.Instance.SelectedDifficulty;
                var initIx = Blockchain.ProgramClient.BuildInitPuzzleIx(
                    session.SessionPublicKey, playerAuthPDA, gameConfigPDA,
                    boardPDA, statsPDA,
                    numTubes, ballsPerTube,
                    difficulty: difficulty,
                    gameMode: 0
                );
                var initTx = await Blockchain.TxBuilder.BuildAsync(initIx, feePayer: session.SessionPublicKey);
                var initSig = await session.SignAndSendL1Async(initTx);
                await SolanaManager.Instance.ConfirmTransactionAsync(initSig);

                // ── Step 3: Wait for VRF ─────────────────────────────────────
                gstate.SetLoadingStep(LoadingStep.WaitingForVRF,
                    "Waiting for oracle to deliver randomness...");
                CurrentPlayerAuth = await Blockchain.VRFPoller.Instance
                    .WaitForBoardReadyAsync(playerAuthPDA);

                // ── Step 4: start_puzzle (L1, BEFORE delegation) ──────────────
                // TypeScript test order: initPuzzle → VRF wait → startPuzzle → delegate.
                // start_puzzle writes to puzzle_board and puzzle_stats on L1.
                // Once those accounts are delegated they are locked on L1 — calling
                // start_puzzle after delegation causes AccountNotSigner (0xbbf).
                gstate.SetLoadingStep(LoadingStep.StartingPuzzle,
                    "Shuffling balls from VRF seed...");
                var startIx = Blockchain.ProgramClient.BuildStartPuzzleIx(
                    session.SessionPublicKey, playerAuthPDA, gameConfigPDA, boardPDA, statsPDA);
                var startTx = await Blockchain.TxBuilder.BuildAsync(startIx, feePayer: session.SessionPublicKey);
                var startSig = await session.SignAndSendL1Async(startTx);
                await SolanaManager.Instance.ConfirmTransactionAsync(startSig);

                // ── Step 5: Create permissions ────────────────────────────────
                gstate.SetLoadingStep(LoadingStep.CreatingPermissions,
                    "Setting up Ephemeral Rollup permissions...");
                await ERManager.Instance.CreatePermissionsAsync(
                    playerAuthPDA, boardPDA, statsPDA, nonce);

                // ── Step 6: Delegate permissions ──────────────────────────────
                gstate.SetLoadingStep(LoadingStep.DelegatingPermissions,
                    "Delegating permissions to ER validator...");
                await ERManager.Instance.DelegatePermissionsAsync(
                    playerAuthPDA, boardPDA, statsPDA, nonce);

                // ── Step 7: Delegate puzzle ───────────────────────────────────
                gstate.SetLoadingStep(LoadingStep.DelegatingPuzzle,
                    "Moving puzzle to Ephemeral Rollup...");
                await ERManager.Instance.DelegatePuzzleAsync(
                    playerAuthPDA, boardPDA, statsPDA, nonce);

                // ── TEE sync wait ─────────────────────────────────────────────
                // Mirrors the TypeScript test's waitUntilPermissionActive().
                gstate.SetLoadingStep(LoadingStep.DelegatingPuzzle,
                    "Waiting for TEE to sync delegation...");
                await System.Threading.Tasks.Task.Delay(5000);

                // ── Fetch initial board state ─────────────────────────────────
                await RefreshBoardAndStatsAsync(playerAuthPDA);

                // No polling loop — board refreshes after each move/undo only.
                gstate.GoToGameActive();
            }
            catch (Exception e)
            {
                gstate.RaiseError($"Failed to start puzzle: {e.Message}");
                gstate.GoToDashboard();
                throw;
            }
        }

        // ── Apply move ────────────────────────────────────────────────────────

        public async Task ApplyMoveAsync(byte fromTube, byte toTube)
        {
            var session = Core.SessionKeyManager.Instance;
            var playerKey = Core.WalletManager.Instance.PublicKey;
            var (playerAuthPDA, _) = Blockchain.PDAHelper.GetPlayerAuthPDA(playerKey);
            var (gameConfigPDA, _) = Blockchain.PDAHelper.GetGameConfigPDA();
            var nonce = CurrentPlayerAuth.PuzzlesStartedNonce;
            var (boardPDA, _) = Blockchain.PDAHelper.GetActivePuzzleBoardPDA(playerAuthPDA, nonce);
            var (statsPDA, _) = Blockchain.PDAHelper.GetActivePuzzleStatsPDA(playerAuthPDA, nonce);

            var ix = Blockchain.ProgramClient.BuildApplyMoveIx(
                session.SessionPublicKey, playerAuthPDA, gameConfigPDA,
                boardPDA, statsPDA, fromTube, toTube);

            var tx = await Blockchain.TxBuilder.BuildAsync(ix, feePayer: session.SessionPublicKey);

            // 1. UPDATE LOCAL STATE IMMEDIATELY (Authority is now here)
            ApplyMoveLocally(fromTube, toTube);

            // 2. SEND TO TEE IN BACKGROUND (Don't 'await' the refresh)
            // This makes the move feel instant while the chain catches up
            await session.SignAndSendErAsync(tx);

            // NOTE: We removed RefreshBoardAndStatsAsync() from here!
        }

        // ── Apply undo ────────────────────────────────────────────────────────

        public async Task ApplyUndoAsync()
        {
            if (!CurrentPuzzleBoard.HasUndo) return;

            var session = Core.SessionKeyManager.Instance;
            var playerKey = Core.WalletManager.Instance.PublicKey;
            var (playerAuthPDA, _) = Blockchain.PDAHelper.GetPlayerAuthPDA(playerKey);
            var (gameConfigPDA, _) = Blockchain.PDAHelper.GetGameConfigPDA();
            var nonce = CurrentPlayerAuth.PuzzlesStartedNonce;
            var (boardPDA, _) = Blockchain.PDAHelper.GetActivePuzzleBoardPDA(playerAuthPDA, nonce);
            var (statsPDA, _) = Blockchain.PDAHelper.GetActivePuzzleStatsPDA(playerAuthPDA, nonce);

            var ix = Blockchain.ProgramClient.BuildApplyUndoIx(
                session.SessionPublicKey, playerAuthPDA, gameConfigPDA, boardPDA, statsPDA);

            var tx = await Blockchain.TxBuilder.BuildAsync(ix, feePayer: session.SessionPublicKey);
            await session.SignAndSendErAsync(tx);

            var (playerAuthPDA2, _) = Blockchain.PDAHelper.GetPlayerAuthPDA(playerKey);
            await RefreshBoardAndStatsAsync(playerAuthPDA2);
        }

        // ── Abandon ───────────────────────────────────────────────────────────

        public async Task AbandonPuzzleAsync()
        {
            var session = Core.SessionKeyManager.Instance;
            var wallet = Core.WalletManager.Instance;
            var playerKey = wallet.PublicKey;
            var (playerAuthPDA, _) = Blockchain.PDAHelper.GetPlayerAuthPDA(playerKey);
            var (gameConfigPDA, _) = Blockchain.PDAHelper.GetGameConfigPDA();
            var nonce = CurrentPlayerAuth?.PuzzlesStartedNonce ?? 0;
            var (boardPDA, _) = Blockchain.PDAHelper.GetActivePuzzleBoardPDA(playerAuthPDA, nonce);
            var (statsPDA, _) = Blockchain.PDAHelper.GetActivePuzzleStatsPDA(playerAuthPDA, nonce);

            // ── Step 1: Undelegate on TEE (flushes ER state → L1) ────────────
            // ERManager.UndelegatePuzzleAsync now sends to TEE and waits 2s internally.
            await ERManager.Instance.UndelegatePuzzleAsync(playerAuthPDA, boardPDA, statsPDA);

            // ── Step 2: Abandon on L1 (signed by session key) ────────────────
            // Matches the test: sessionKeypair signs abandonPuzzle on l1Connection.
            var ix = Blockchain.ProgramClient.BuildAbandonPuzzleIx(
                session.SessionPublicKey, playerAuthPDA, gameConfigPDA);
            var tx = await Blockchain.TxBuilder.BuildAsync(ix, feePayer: session.SessionPublicKey);
            var sig = await session.SignAndSendL1Async(tx);
            await SolanaManager.Instance.ConfirmTransactionAsync(sig);
            Debug.Log($"[Orch] Puzzle abandoned. Sig: {sig}");

            // ── Step 3: Close session (player wallet, L1) ────────────────────
            await Core.SessionKeyManager.Instance.CloseSessionAsync();
            GameStateManager.Instance.GoToDashboard();
        }

        // ── Finalize ──────────────────────────────────────────────────────────

        public async Task FinalizePuzzleAsync()
        {
            var gstate = GameStateManager.Instance;
            var wallet = Core.WalletManager.Instance;
            var playerKey = wallet.PublicKey;

            gstate.GoToFinalizing();

            try
            {
                var (playerAuthPDA, _) = Blockchain.PDAHelper.GetPlayerAuthPDA(playerKey);
                var (gameConfigPDA, _) = Blockchain.PDAHelper.GetGameConfigPDA();
                var nonce = CurrentPlayerAuth.PuzzlesStartedNonce;
                var (boardPDA, _) = Blockchain.PDAHelper.GetActivePuzzleBoardPDA(playerAuthPDA, nonce);
                var (statsPDA, _) = Blockchain.PDAHelper.GetActivePuzzleStatsPDA(playerAuthPDA, nonce);

                // ── Undelegate (commit ER → L1) ───────────────────────────────
                gstate.SetLoadingStep(LoadingStep.Undelegating,
                    "Committing moves back to Solana L1...");
                await ERManager.Instance.UndelegatePuzzleAsync(
                    playerAuthPDA, boardPDA, statsPDA);

                // ── finalize_puzzle ───────────────────────────────────────────
                // Signed by SESSION KEY — consistent with all other gameplay txs.
                // ERManager.UndelegatePuzzleAsync (above) now sends to TEE + waits 2s.
                gstate.SetLoadingStep(LoadingStep.Finalizing,
                    "Recording score on-chain...");
                var session = Core.SessionKeyManager.Instance;
                var ix = Blockchain.ProgramClient.BuildFinalizePuzzleIx(
                    session.SessionPublicKey, playerAuthPDA, gameConfigPDA, statsPDA);

                var tx = await Blockchain.TxBuilder.BuildAsync(ix, feePayer: session.SessionPublicKey);
                var sig = await session.SignAndSendL1Async(tx);
                await SolanaManager.Instance.ConfirmTransactionAsync(sig);

                // ── Close session ─────────────────────────────────────────────
                gstate.SetLoadingStep(LoadingStep.ClosingSession, "Closing session key...");
                await Core.SessionKeyManager.Instance.CloseSessionAsync();

                gstate.GoToDashboard();
            }
            catch (Exception e)
            {
                gstate.RaiseError($"Finalize failed: {e.Message}");
                gstate.GoToDashboard();
                throw;
            }
        }

        // ── Board refresh (called after each move/undo) ──────────────────────

        private async Task RefreshBoardAndStatsAsync(
            Solana.Unity.Wallet.PublicKey playerAuthPDA)
        {
            var nonce = CurrentPlayerAuth.PuzzlesStartedNonce;
            var (boardPDA, _) = Blockchain.PDAHelper.GetActivePuzzleBoardPDA(playerAuthPDA, nonce);
            var (statsPDA, _) = Blockchain.PDAHelper.GetActivePuzzleStatsPDA(playerAuthPDA, nonce);

            var boardData = await SolanaManager.Instance.GetAccountDataAsync(boardPDA, useEr: true);
            var statsData = await SolanaManager.Instance.GetAccountDataAsync(statsPDA, useEr: true);

            if (boardData != null)
                CurrentPuzzleBoard = Blockchain.AccountDeserializer.DeserializePuzzleBoard(boardData);
            if (statsData != null)
                CurrentPuzzleStats = Blockchain.AccountDeserializer.DeserializePuzzleStats(statsData);

            UnityMainThread.Enqueue(() =>
            {
                Board3DManager.Instance?.DrawBoard(CurrentPuzzleBoard);
            });

            // Check for puzzle solved after each refresh
            if (CurrentPuzzleStats?.IsSolved == true)
            {
                UnityMainThread.Enqueue(() =>
                    GameStateManager.Instance.GoToGameSolved());
            }
        }

        // ── Optimistic local update ───────────────────────────────────────────

        private void ApplyMoveLocally(byte from, byte to)
        {
            if (CurrentPuzzleBoard == null) return;
            var board = CurrentPuzzleBoard;
            int fromLen = board.TubeLengths[from];
            int toLen = board.TubeLengths[to];
            int baseFrom = from * Blockchain.PuzzleConstants.MAX_CAPACITY;
            int baseTo = to * Blockchain.PuzzleConstants.MAX_CAPACITY;

            byte ball = board.Balls[baseFrom + fromLen - 1];
            board.Balls[baseFrom + fromLen - 1] = 0;
            board.TubeLengths[from]--;
            board.Balls[baseTo + toLen] = ball;
            board.TubeLengths[to]++;
            board.HasUndo = true;
            board.UndoFrom = from;
            board.UndoTo = to;
            board.UndoBall = ball;

            UnityMainThread.Enqueue(() =>
            {
                Board3DManager.Instance?.DrawBoard(CurrentPuzzleBoard);
            });

            // Check for puzzle solved after each refresh
            if (CurrentPuzzleStats?.IsSolved == true)
            {
                UnityMainThread.Enqueue(() =>
                    GameStateManager.Instance.GoToGameSolved());
            }
        }

        private void OnDestroy() { }
    }
}