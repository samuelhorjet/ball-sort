using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Solana.Unity.Wallet;
using BallSort.Core;

namespace BallSort.Blockchain
{
    /// <summary>
    /// Polls the PlayerAuth account every 2 seconds until
    /// activePuzzleStatus == BoardReady (1), meaning the VRF oracle
    /// has delivered randomness and consume_randomness has been called.
    /// </summary>
    public class VRFPoller : MonoBehaviour
    {
        public static VRFPoller Instance { get; private set; }

        private const int POLL_INTERVAL_MS = 2000;
        private const int MAX_ATTEMPTS     = 60;  // 2 min timeout

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Polls until VRF randomness is delivered.
        /// Returns the updated PlayerAuth account on success.
        /// Throws TimeoutException if oracle doesn't respond within MAX_ATTEMPTS.
        /// </summary>
        public async Task<PlayerAuth> WaitForBoardReadyAsync(
            PublicKey playerAuthPDA,
            CancellationToken ct = default)
        {
            Debug.Log("[VRF] Polling for BoardReady...");

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(POLL_INTERVAL_MS, ct);

                try
                {
                    var data = await SolanaManager.Instance.GetAccountDataAsync(playerAuthPDA);
                    if (data == null) continue;

                    var auth = AccountDeserializer.DeserializePlayerAuth(data);
                    Debug.Log($"[VRF] Poll {attempt + 1}: status={auth.ActivePuzzleStatus}");

                    if (auth.ActivePuzzleStatus == (byte)PuzzleStatus.BoardReady)
                    {
                        Debug.Log("[VRF] BoardReady! Randomness delivered.");
                        return auth;
                    }
                }
                catch (Exception e) when (!(e is OperationCanceledException))
                {
                    Debug.LogWarning($"[VRF] Poll error (attempt {attempt + 1}): {e.Message}");
                }
            }

            throw new TimeoutException(
                "VRF oracle did not respond within the timeout period. " +
                "Please check your network connection and try again.");
        }
    }
}
