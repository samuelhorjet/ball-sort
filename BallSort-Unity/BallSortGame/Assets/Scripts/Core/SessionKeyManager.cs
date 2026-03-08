using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Programs;       // ← SystemProgram.Transfer
using BallSort.Blockchain;

namespace BallSort.Core
{
    /// <summary>
    /// Manages the ephemeral session keypair used to sign all in-game transactions
    /// without wallet popups.
    ///
    /// For ER transactions (applyMove, applyUndo, undelegatePuzzle):
    ///   SignAndSendErAsync() passes the UNSIGNED Transaction to
    ///   SolanaManager.SendErTransactionAsync(), which:
    ///     1. Gets a fresh auth token from TEE
    ///     2. Fetches the blockhash FROM the TEE connection (not L1)
    ///     3. Sets tx.RecentBlockHash, builds + signs, then sends
    ///
    ///   This mirrors the TypeScript test exactly:
    ///     const authToken = await getAuthToken(endpoint, sessionKeypair.publicKey, signerFn);
    ///     const teeConnection = new Connection(`${TEE_URL}?token=${authToken.token}`, ...);
    ///     tx.recentBlockhash = (await teeConnection.getLatestBlockhash()).blockhash;
    ///     await sendAndConfirmTransaction(teeConnection, tx, [sessionKeypair], { skipPreflight: true });
    /// </summary>
    public class SessionKeyManager : MonoBehaviour
    {
        public static SessionKeyManager Instance { get; private set; }

        public Account   SessionAccount   { get; private set; }
        public bool      HasActiveSession => SessionAccount != null;
        public PublicKey SessionPublicKey => SessionAccount?.PublicKey;

        private const uint  SESSION_DURATION_SECS    = 3600;
        private const ulong SESSION_KEY_FUND_LAMPORTS = 50_000_000; // 0.05 SOL

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task OpenSessionAsync()
        {
            SessionAccount = new Account();
            Debug.Log($"[Session] Generated keypair: {SessionAccount.PublicKey}");

            var wallet    = WalletManager.Instance;
            var playerKey = wallet.PublicKey;
            var (playerAuthPDA, _) = PDAHelper.GetPlayerAuthPDA(playerKey);

            // ── Step A: Fund the session key ──────────────────────────────────
            var fundIx  = SystemProgram.Transfer(playerKey, SessionAccount.PublicKey, SESSION_KEY_FUND_LAMPORTS);
            var fundTx  = await TxBuilder.BuildAsync(fundIx, feePayer: playerKey);
            var fundSig = await wallet.SignAndSendAsync(fundTx);
            await SolanaManager.Instance.ConfirmTransactionAsync(fundSig);
            Debug.Log($"[Session] Funded session key with {SESSION_KEY_FUND_LAMPORTS} lamports.");

            // ── Step B: Open session on-chain ─────────────────────────────────
            var ix  = ProgramClient.BuildOpenSessionIx(playerKey, playerAuthPDA,
                SessionAccount.PublicKey, SESSION_DURATION_SECS);
            var tx  = await TxBuilder.BuildAsync(ix, feePayer: playerKey);
            var sig = await wallet.SignAndSendAsync(tx);
            await SolanaManager.Instance.ConfirmTransactionAsync(sig);
            Debug.Log($"[Session] Opened on-chain. Sig: {sig}");

            // NOTE: No auth call here. Auth tokens are obtained fresh per ER transaction
            // inside SendErTransactionAsync() — exactly like the TypeScript test.
        }

        /// <summary>
        /// Sign a transaction with the session keypair and send to Solana L1.
        /// Blockhash is fetched from L1 via TxBuilder.BuildAsync before this is called.
        /// Used for: initPuzzle, startPuzzle, createPermissions, delegatePermissions,
        ///           delegatePuzzle, abandonPuzzle (L1 part), finalizePuzzle.
        /// </summary>
        public async Task<string> SignAndSendL1Async(Transaction tx)
        {
            if (!HasActiveSession)
                throw new InvalidOperationException("No active session");

            var raw = tx.Build(new List<Account> { SessionAccount });
            return await SolanaManager.Instance.SendRawTransactionAsync(raw);
        }

        /// <summary>
        /// Send a transaction to the TEE ephemeral rollup via the session keypair.
        ///
        /// IMPORTANT: Pass the transaction UNSIGNED (no blockhash set yet).
        /// SolanaManager.SendErTransactionAsync() will:
        ///   1. Get a fresh auth token from TEE (never cached)
        ///   2. Fetch blockhash FROM the TEE connection — mirrors the test:
        ///        tx.recentBlockhash = (await teeConnection.getLatestBlockhash()).blockhash
        ///   3. Build + sign with SessionAccount
        ///   4. Send via TEE client with skipPreflight = true
        ///
        /// Used for: applyMove, applyUndo, undelegatePuzzle.
        /// </summary>
        public async Task<string> SignAndSendErAsync(Transaction tx)
        {
            if (!HasActiveSession)
                throw new InvalidOperationException("No active session");

            // Pass the raw Transaction — NOT pre-built bytes.
            // SolanaManager will set the TEE blockhash, then build + sign + send.
            return await SolanaManager.Instance.SendErTransactionAsync(tx, SessionAccount);
        }

        public async Task CloseSessionAsync()
        {
            if (!HasActiveSession) return;

            try
            {
                var wallet    = WalletManager.Instance;
                var playerKey = wallet.PublicKey;
                var (playerAuthPDA, _) = PDAHelper.GetPlayerAuthPDA(playerKey);

                var ix  = ProgramClient.BuildCloseSessionIx(playerKey, playerAuthPDA);
                var tx  = await TxBuilder.BuildAsync(ix, feePayer: playerKey);
                var sig = await wallet.SignAndSendAsync(tx);
                await SolanaManager.Instance.ConfirmTransactionAsync(sig);
                Debug.Log($"[Session] Closed on-chain. Sig: {sig}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Session] Close failed (ignoring): {e.Message}");
            }
            finally
            {
                SessionAccount = null;
            }
        }
    }
}