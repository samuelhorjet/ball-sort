using System;
using System.Threading.Tasks;
using UnityEngine;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using BallSort.Core;
using BallSort.Blockchain;   // ← adds TxBuilder, ProgramClient, PDAHelper into scope

namespace BallSort.Game
{
    public class ERManager : MonoBehaviour
    {
        public static ERManager Instance { get; private set; }

        private static readonly PublicKey PERMISSION_PROGRAM =
            new PublicKey("ACLseoPoyC3cBqoUtkbjZ4aDrkurZW86v19pXz2XQnp1");
        private static readonly PublicKey DELEGATION_PROGRAM =
            new PublicKey("DELeGGvXpWV2fqJUhqcF5ZSYMS4JTLjteaAMARRSaeSh");
        private static readonly PublicKey ER_VALIDATOR =
            new PublicKey("FnE6VJT5QNZdedZPnCoLsARgBwoE6DeJNjBs2H1gySXA");
        private static readonly PublicKey MAGIC_PROGRAM =
            new PublicKey("Magic11111111111111111111111111111111111111");
        private static readonly PublicKey MAGIC_CONTEXT =
            new PublicKey("MagicContext1111111111111111111111111111111");

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task CreatePermissionsAsync(PublicKey playerAuthPDA, PublicKey boardPDA, PublicKey statsPDA, ulong nonce)
        {
            var session    = Core.SessionKeyManager.Instance;
            var sessionKey = session.SessionPublicKey;

            var (boardPermPDA, _) = DerivePermissionPDA(boardPDA);
            var (statsPermPDA, _) = DerivePermissionPDA(statsPDA);

            var ix = ProgramClient.BuildCreatePuzzlePermissionsIx(
                payer:             sessionKey,
                playerAuth:        playerAuthPDA,
                gameConfig:        PDAHelper.GetGameConfigPDA().pda,
                puzzleBoard:       boardPDA,
                puzzleStats:       statsPDA,
                boardPermission:   boardPermPDA,
                statsPermission:   statsPermPDA,
                permissionProgram: PERMISSION_PROGRAM
            );

            var tx  = await TxBuilder.BuildAsync(ix, feePayer: sessionKey);
            var sig = await session.SignAndSendL1Async(tx);
            await SolanaManager.Instance.ConfirmTransactionAsync(sig);
            Debug.Log($"[ER] Permissions created. Sig: {sig}");
        }

        public async Task DelegatePermissionsAsync(PublicKey playerAuthPDA, PublicKey boardPDA, PublicKey statsPDA, ulong nonce)
        {
            var session    = Core.SessionKeyManager.Instance;
            var sessionKey = session.SessionPublicKey;

            var (boardPermPDA, _) = DerivePermissionPDA(boardPDA);
            var (statsPermPDA, _) = DerivePermissionPDA(statsPDA);

            var (boardDelegBuf, _)  = DeriveBufferPDA(boardPermPDA, PERMISSION_PROGRAM);
            var (boardDelegRec, _)  = DeriveRecordPDA(boardPermPDA);
            var (boardDelegMeta, _) = DeriveMetadataPDA(boardPermPDA);
            var (statsDelegBuf, _)  = DeriveBufferPDA(statsPermPDA, PERMISSION_PROGRAM);
            var (statsDelegRec, _)  = DeriveRecordPDA(statsPermPDA);
            var (statsDelegMeta, _) = DeriveMetadataPDA(statsPermPDA);

            var ix = ProgramClient.BuildDelegatePuzzlePermissionsIx(
                payer:             sessionKey,
                playerAuth:        playerAuthPDA,
                gameConfig:        PDAHelper.GetGameConfigPDA().pda,
                puzzleBoard:       boardPDA,
                puzzleStats:       statsPDA,
                boardPermission:   boardPermPDA,
                statsPermission:   statsPermPDA,
                permissionProgram: PERMISSION_PROGRAM,
                delegationProgram: DELEGATION_PROGRAM,
                boardDelegBuffer:  boardDelegBuf,
                boardDelegRecord:  boardDelegRec,
                boardDelegMeta:    boardDelegMeta,
                statsDelegBuffer:  statsDelegBuf,
                statsDelegRecord:  statsDelegRec,
                statsDelegMeta:    statsDelegMeta,
                validator:         ER_VALIDATOR
            );

            var tx  = await TxBuilder.BuildAsync(ix, feePayer: sessionKey);
            var sig = await session.SignAndSendL1Async(tx);
            await SolanaManager.Instance.ConfirmTransactionAsync(sig);
            Debug.Log($"[ER] Permissions delegated. Sig: {sig}");
        }

        public async Task DelegatePuzzleAsync(PublicKey playerAuthPDA, PublicKey boardPDA, PublicKey statsPDA, ulong nonce)
        {
            var session    = Core.SessionKeyManager.Instance;
            var sessionKey = session.SessionPublicKey;

            // Buffer PDAs: delegateBufferPdaFromDelegatedAccountAndOwnerProgram(puzzle, ProgramId)
            var (boardDelegBuf, _)  = DeriveBufferPDA(boardPDA, ProgramClient.ProgramId);
            var (statsDelegBuf, _)  = DeriveBufferPDA(statsPDA, ProgramClient.ProgramId);

            // ── Record/Metadata PDAs: seeds = ["delegation"/"delegation-metadata", account] ──
            // These correctly use DELEGATION_PROGRAM (unchanged)
            var (boardDelegRec, _)  = DeriveRecordPDA(boardPDA);
            var (boardDelegMeta, _) = DeriveMetadataPDA(boardPDA);
            var (statsDelegRec, _)  = DeriveRecordPDA(statsPDA);
            var (statsDelegMeta, _) = DeriveMetadataPDA(statsPDA);

            var ix = ProgramClient.BuildDelegatePuzzleIx(
                payer:             sessionKey,
                playerAuth:        playerAuthPDA,
                gameConfig:        PDAHelper.GetGameConfigPDA().pda,
                bufferBoard:       boardDelegBuf,
                recordBoard:       boardDelegRec,
                metaBoard:         boardDelegMeta,
                puzzleBoard:       boardPDA,
                bufferStats:       statsDelegBuf,
                recordStats:       statsDelegRec,
                metaStats:         statsDelegMeta,
                puzzleStats:       statsPDA,
                validator:         ER_VALIDATOR,
                ownerProgram:      ProgramClient.ProgramId,
                delegationProgram: DELEGATION_PROGRAM
            );

            var tx  = await TxBuilder.BuildAsync(ix, feePayer: sessionKey);
            var sig = await session.SignAndSendL1Async(tx);
            await SolanaManager.Instance.ConfirmTransactionAsync(sig);
            Debug.Log($"[ER] Puzzle delegated to ER. Sig: {sig}");
        }

        public async Task UndelegatePuzzleAsync(PublicKey playerAuthPDA, PublicKey boardPDA, PublicKey statsPDA)
        {
            var session    = Core.SessionKeyManager.Instance;
            var sessionKey = session.SessionPublicKey;

            var ix = ProgramClient.BuildUndelegatePuzzleIx(
                payer:        sessionKey,
                playerAuth:   playerAuthPDA,
                gameConfig:   PDAHelper.GetGameConfigPDA().pda,
                puzzleBoard:  boardPDA,
                puzzleStats:  statsPDA,
                magicProgram: MAGIC_PROGRAM,
                magicContext: MAGIC_CONTEXT
            );

            // ── IMPORTANT: undelegatePuzzle must go to the TEE/ER, not L1 ────────────
            // This tells the ER validator to commit accumulated state back to Solana L1.
            // After sending, wait ~2s for L1 to sync (mirrors the TypeScript test's sleep).
            var tx = await TxBuilder.BuildAsync(ix, feePayer: sessionKey);
            await session.SignAndSendErAsync(tx);
            Debug.Log("[ER] Undelegate sent to TEE. Waiting 2s for L1 sync...");
            await System.Threading.Tasks.Task.Delay(2000);
            Debug.Log("[ER] Puzzle undelegated (committed to L1).");
        }

        // ── MagicBlock PDA Derivations — derived directly from pda.js SDK source ──

        private (PublicKey, byte) DerivePermissionPDA(PublicKey account)
        {
            // pda.js: PERMISSION_SEED = Buffer.from("permission:") ← colon is required
            // findProgramAddressSync([PERMISSION_SEED, account.toBuffer()], PERMISSION_PROGRAM_ID)
            PublicKey.TryFindProgramAddress(
                new[] { System.Text.Encoding.UTF8.GetBytes("permission:"), account.KeyBytes },
                PERMISSION_PROGRAM, out var pda, out var bump);
            return (pda, bump);
        }

        /// <summary>
        /// pda.js: delegateBufferPdaFromDelegatedAccountAndOwnerProgram(account, ownerProgram)
        /// → findProgramAddressSync(["buffer", account.toBytes()], ownerProgramId)
        /// Seeds = ["buffer", account] — 2 seeds only. Program = ownerProgram (NOT delegation).
        /// Used as DeriveBufferPDA(permissionPDA, PERMISSION_PROGRAM) for delegate_puzzle_permissions,
        /// and DeriveBufferPDA(puzzleBoard, ProgramId) for delegate_puzzle.
        /// </summary>
        private (PublicKey, byte) DeriveBufferPDA(PublicKey account, PublicKey ownerProgram)
        {
            PublicKey.TryFindProgramAddress(
                new[] { System.Text.Encoding.UTF8.GetBytes("buffer"), account.KeyBytes },
                ownerProgram, out var pda, out var bump);
            return (pda, bump);
        }

        private (PublicKey, byte) DeriveRecordPDA(PublicKey account)
        {
            // pda.js: delegationRecordPdaFromDelegatedAccount
            // → findProgramAddressSync(["delegation", account.toBytes()], DELEGATION_PROGRAM_ID)
            PublicKey.TryFindProgramAddress(
                new[] { System.Text.Encoding.UTF8.GetBytes("delegation"), account.KeyBytes },
                DELEGATION_PROGRAM, out var pda, out var bump);
            return (pda, bump);
        }

        private (PublicKey, byte) DeriveMetadataPDA(PublicKey account)
        {
            // pda.js: delegationMetadataPdaFromDelegatedAccount
            // → findProgramAddressSync(["delegation-metadata", account.toBytes()], DELEGATION_PROGRAM_ID)
            PublicKey.TryFindProgramAddress(
                new[] { System.Text.Encoding.UTF8.GetBytes("delegation-metadata"), account.KeyBytes },
                DELEGATION_PROGRAM, out var pda, out var bump);
            return (pda, bump);
        }
    }
}