using System;
using Solana.Unity.Wallet;

namespace BallSort.Blockchain
{
    /// <summary>
    /// Deserializes raw Solana account data (Anchor-encoded) into typed C# structs.
    /// Anchor accounts start with an 8-byte discriminator, then borsh-encoded fields.
    /// Field order matches the Rust struct definitions exactly.
    /// </summary>
    public static class AccountDeserializer
    {
        private const int DISCRIMINATOR_SIZE = 8;

        // ── PlayerAuth ────────────────────────────────────────────────────────
        public static PlayerAuth DeserializePlayerAuth(byte[] data)
        {
            int offset = DISCRIMINATOR_SIZE;
            var auth = new PlayerAuth
            {
                Wallet              = ReadPublicKey(data, ref offset),
                SessionKey          = ReadOptionPublicKey(data, ref offset),
                SessionExpiresAt    = ReadI64(data, ref offset),
                HasActivePuzzle     = ReadBool(data, ref offset),
                ActivePuzzleEntity  = ReadOptionPublicKey(data, ref offset),
                ActivePuzzleStatus  = ReadU8(data, ref offset),
                TotalPuzzlesSolved  = ReadU64(data, ref offset),
                SoarPlayerAccount   = ReadPublicKey(data, ref offset),
                PuzzlesStartedNonce = ReadU64(data, ref offset),
                VrfRandomness       = ReadBytes(data, ref offset, 32),
                PuzzleNumTubes      = ReadU8(data, ref offset),
                PuzzleBallsPerTube  = ReadU8(data, ref offset),
                PuzzleDifficulty    = ReadU8(data, ref offset),
                Bump                = ReadU8(data, ref offset),
            };
            return auth;
        }

        // ── PuzzleBoard ───────────────────────────────────────────────────────
        public static PuzzleBoard DeserializePuzzleBoard(byte[] data)
        {
            int offset = DISCRIMINATOR_SIZE;
            return new PuzzleBoard
            {
                NumTubes    = ReadU8(data, ref offset),
                NumColors   = ReadU8(data, ref offset),
                MaxCapacity = ReadU8(data, ref offset),
                Balls       = ReadBytes(data, ref offset, PuzzleConstants.BALLS_LEN),
                TubeLengths = ReadBytes(data, ref offset, PuzzleConstants.MAX_TUBES),
                VrfSeed     = ReadBytes(data, ref offset, 32),
                UndoFrom    = ReadU8(data, ref offset),
                UndoTo      = ReadU8(data, ref offset),
                UndoBall    = ReadU8(data, ref offset),
                HasUndo     = ReadBool(data, ref offset),
            };
        }

        // ── PuzzleStats ───────────────────────────────────────────────────────
        public static PuzzleStats DeserializePuzzleStats(byte[] data)
        {
            int offset = DISCRIMINATOR_SIZE;
            return new PuzzleStats
            {
                Status      = ReadU8(data, ref offset),
                Difficulty  = ReadU8(data, ref offset),
                NumTubes    = ReadU8(data, ref offset),
                BallsPerTube= ReadU8(data, ref offset),
                MoveCount   = ReadU32(data, ref offset),
                UndoCount   = ReadU32(data, ref offset),
                StartedAt   = ReadI64(data, ref offset),
                CompletedAt = ReadI64(data, ref offset),
                IsSolved    = ReadBool(data, ref offset),
                FinalScore  = ReadU64(data, ref offset),
            };
        }

        // ── PlayerProfile ─────────────────────────────────────────────────────
        public static PlayerProfile DeserializePlayerProfile(byte[] data)
        {
            int offset = DISCRIMINATOR_SIZE;
            return new PlayerProfile
            {
                TotalPuzzlesStarted = ReadU32(data, ref offset),
                TotalPuzzlesSolved  = ReadU32(data, ref offset),
                BestScoreEasy       = ReadU64(data, ref offset),
                BestScoreMedium     = ReadU64(data, ref offset),
                BestScoreHard       = ReadU64(data, ref offset),
            };
        }

        // ── Tournament ────────────────────────────────────────────────────────
        public static Tournament DeserializeTournament(byte[] data)
        {
            int offset = DISCRIMINATOR_SIZE;
            return new Tournament
            {
                Authority       = ReadPublicKey(data, ref offset),
                EntryFee        = ReadU64(data, ref offset),
                PrizePool       = ReadU64(data, ref offset),
                NetPrizePool    = ReadU64(data, ref offset),
                TreasuryFeeBps  = ReadU16(data, ref offset),
                Difficulty      = ReadU8(data, ref offset),
                StartTime       = ReadI64(data, ref offset),
                EndTime         = ReadI64(data, ref offset),
                TotalEntries    = ReadU32(data, ref offset),
                TotalCompleters = ReadU32(data, ref offset),
                IsClosed        = ReadBool(data, ref offset),
                TournamentId    = ReadU64(data, ref offset),
                Bump            = ReadU8(data, ref offset),
            };
        }

        // ── Primitive readers (little-endian, Borsh) ─────────────────────────

        private static byte ReadU8(byte[] data, ref int offset)
            => data[offset++];

        private static ushort ReadU16(byte[] data, ref int offset)
        {
            var v = BitConverter.ToUInt16(data, offset);
            offset += 2;
            return v;
        }

        private static uint ReadU32(byte[] data, ref int offset)
        {
            var v = BitConverter.ToUInt32(data, offset);
            offset += 4;
            return v;
        }

        private static ulong ReadU64(byte[] data, ref int offset)
        {
            var v = BitConverter.ToUInt64(data, offset);
            offset += 8;
            return v;
        }

        private static long ReadI64(byte[] data, ref int offset)
        {
            var v = BitConverter.ToInt64(data, offset);
            offset += 8;
            return v;
        }

        private static bool ReadBool(byte[] data, ref int offset)
            => data[offset++] != 0;

        private static byte[] ReadBytes(byte[] data, ref int offset, int length)
        {
            var bytes = new byte[length];
            Array.Copy(data, offset, bytes, 0, length);
            offset += length;
            return bytes;
        }

        private static PublicKey ReadPublicKey(byte[] data, ref int offset)
        {
            var keyBytes = ReadBytes(data, ref offset, 32);
            return new PublicKey(keyBytes);
        }

        /// <summary>Reads Borsh Option<PublicKey>: 1 byte tag (0=None, 1=Some) + optional 32 bytes.</summary>
        private static PublicKey ReadOptionPublicKey(byte[] data, ref int offset)
        {
            byte tag = ReadU8(data, ref offset);
            if (tag == 0) return null;
            return ReadPublicKey(data, ref offset);
        }
    }
}
