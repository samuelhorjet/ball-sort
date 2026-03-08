using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using BallSort.Core;

namespace BallSort.Blockchain
{
    /// <summary>
    /// Builds unsigned Transaction objects ready for wallet signing.
    /// UnityMainThread has been moved to its own file: UnityMainThread.cs
    /// </summary>
    public static class TxBuilder
    {
        public static async Task<Transaction> BuildAsync(
            TransactionInstruction ix,
            PublicKey feePayer,
            params TransactionInstruction[] extraIxs)
        {
            var blockhash = await SolanaManager.Instance.GetLatestBlockhashAsync();

            var ixs = new List<TransactionInstruction> { ix };
            if (extraIxs != null && extraIxs.Length > 0)
                ixs.AddRange(extraIxs);

            return new Transaction
            {
                RecentBlockHash = blockhash,
                FeePayer        = feePayer,
                Instructions    = ixs,
                Signatures      = new List<SignaturePubKeyPair>()
            };
        }

        public static async Task<Transaction> BuildAsync(
            List<TransactionInstruction> ixs,
            PublicKey feePayer)
        {
            var blockhash = await SolanaManager.Instance.GetLatestBlockhashAsync();

            return new Transaction
            {
                RecentBlockHash = blockhash,
                FeePayer        = feePayer,
                Instructions    = ixs,
                Signatures      = new List<SignaturePubKeyPair>()
            };
        }
    }

    // ── Constants ─────────────────────────────────────────────────────────────
    public static class Constants
    {
        public const string VRF_ORACLE_QUEUE =
            "Cuj97ggrhhidhbu39TijNVqE74xvKJ69gDervRUXAxGh";

        public const string ER_VALIDATOR =
            "FnE6VJT5QNZdedZPnCoLsARgBwoE6DeJNjBs2H1gySXA";

        public const int MIN_TUBES = 4;
        public const int MAX_TUBES = 10;
        public const int MIN_BALLS = 4;
        public const int MAX_BALLS = 10;

        public const uint SESSION_DURATION_SECS = 3600;
    }
}