using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using Solana.Unity.KeyStore;
using Solana.Unity.Rpc.Models;

namespace BallSort.Core
{
    /// <summary>
    /// Manages the player wallet using the base solana-unity-sdk packages only
    /// (no Solana.Unity.SDK / Web3 required).
    ///
    /// ── How signing works ────────────────────────────────────────────────────
    /// The wallet Account is set via one of:
    ///   • LoadFromMnemonic()   – editor / devnet testing
    ///   • InjectAccount()      – called by your deep-link / MWA bridge after
    ///                            the user approves the connection in their wallet app
    ///
    /// Transactions are signed locally with tx.Build(signers) and sent through
    /// SolanaManager, so no SDK-level popup wrapper is needed.
    /// ─────────────────────────────────────────────────────────────────────────
    /// </summary>
    public class WalletManager : MonoBehaviour
    {
        public static WalletManager Instance { get; private set; }

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<PublicKey> OnWalletConnected;
        public event Action           OnWalletDisconnected;

        // ── Internal state ────────────────────────────────────────────────────
        private Account _account;

        // ── Public accessors ──────────────────────────────────────────────────
        public bool      IsConnected => _account != null;
        public PublicKey PublicKey   => _account?.PublicKey;

        /// <summary>Short display address e.g. "5Ax3...9kRz"</summary>
        public string ShortAddress =>
            IsConnected ? $"{PublicKey.Key[..4]}...{PublicKey.Key[^4..]}" : "";

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Connection ────────────────────────────────────────────────────────

        /// <summary>
        /// Inject a fully-constructed Account from your deep-link / MWA bridge.
        /// Call this after the user approves the wallet connection popup.
        /// </summary>
        public void InjectAccount(Account account)
        {
            _account = account ?? throw new ArgumentNullException(nameof(account));
            Debug.Log($"[Wallet] Account injected: {_account.PublicKey}");
            OnWalletConnected?.Invoke(_account.PublicKey);
        }

        /// <summary>
        /// Parameterless overload called from UI buttons.
        /// In production: hook this up to your deep-link / MWA bridge, then
        /// call InjectAccount() once the user approves in their wallet app.
        /// In the editor: call ConnectAsync(mnemonic) directly instead.
        /// </summary>
        public Task ConnectAsync()
        {
            // TODO: trigger your platform deep-link / MWA flow here and
            //       call InjectAccount(account) in the callback.
            Debug.Log("[Wallet] ConnectAsync() called — awaiting InjectAccount() from wallet bridge.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dev / editor helper — restore a wallet from a BIP-39 mnemonic.
        /// Do NOT use this path in a production build.
        /// </summary>
        public Task ConnectAsync(string mnemonic, string passphrase = "")
        {
            try
            {
                var mnemonicObj = new Mnemonic(mnemonic, WordList.English);
                var wallet      = new Wallet(mnemonicObj, passphrase);
                _account   = wallet.GetAccount(0);
                Debug.Log($"[Wallet] Connected via mnemonic: {_account.PublicKey}");
                OnWalletConnected?.Invoke(_account.PublicKey);
            }
            catch (Exception e)
            {
                GameStateManager.Instance.RaiseError($"Wallet connection failed: {e.Message}");
                throw;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnects the current wallet session.
        /// </summary>
        public Task DisconnectAsync()
        {
            _account = null;
            OnWalletDisconnected?.Invoke();
            Debug.Log("[Wallet] Disconnected");
            return Task.CompletedTask;
        }

        // ── Transaction signing ───────────────────────────────────────────────

        /// <summary>
        /// Sign a Transaction locally using the stored keypair.
        /// Returns the serialized signed bytes.
        /// </summary>
        public Task<byte[]> SignTransactionAsync(Transaction tx)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Wallet not connected");

            var signed = tx.Build(new List<Account> { _account });
            return Task.FromResult(signed);
        }

        /// <summary>
        /// Sign and broadcast a Transaction in one call.
        /// </summary>
        public async Task<string> SignAndSendAsync(Transaction tx)
        {
            var signedBytes = await SignTransactionAsync(tx);
            return await SolanaManager.Instance.SendRawTransactionAsync(signedBytes);
        }
    }
}