using System.Text;
using Solana.Unity.Wallet;
using Solana.Unity.Programs;

namespace BallSort.Blockchain
{
    /// <summary>
    /// Derives all Program Derived Addresses used by the ball_sort program.
    /// Mirrors the TypeScript pdas.ts exactly.
    /// </summary>
    public static class PDAHelper
    {
        // ── Seed constants ────────────────────────────────────────────────────
        private static readonly byte[] SEED_IDENTITY          = Encoding.UTF8.GetBytes("identity");
        private static readonly byte[] SEED_GAME_CONFIG       = Encoding.UTF8.GetBytes("game_config");
        private static readonly byte[] SEED_PLAYER_AUTH       = Encoding.UTF8.GetBytes("player_auth");
        private static readonly byte[] SEED_PLAYER_PROFILE    = Encoding.UTF8.GetBytes("player_profile");
        private static readonly byte[] SEED_PUZZLE_BOARD      = Encoding.UTF8.GetBytes("puzzle_board");
        private static readonly byte[] SEED_PUZZLE_STATS      = Encoding.UTF8.GetBytes("puzzle_stats");
        private static readonly byte[] SEED_TOURNAMENT        = Encoding.UTF8.GetBytes("tournament");
        private static readonly byte[] SEED_TOURNAMENT_ENTRY  = Encoding.UTF8.GetBytes("tournament_entry");

        public static readonly PublicKey ProgramId =
            new PublicKey("EJK22df4w6tBXpavJvFMEJS9RRpp73WTK5FpZbCNappN");

        // ── PDA derivations ───────────────────────────────────────────────────

        /// <summary>
        /// Derives the program_identity PDA (seed = "identity").
        /// Required by init_puzzle to authorize the VRF callback.
        /// </summary>
        public static (PublicKey pda, byte bump) GetProgramIdentityPDA()
        {
            PublicKey.TryFindProgramAddress(
                new[] { SEED_IDENTITY },
                ProgramId,
                out var pda, out var bump);
            return (pda, bump);
        }

        public static (PublicKey pda, byte bump) GetGameConfigPDA()
        {
            PublicKey.TryFindProgramAddress(
                new[] { SEED_GAME_CONFIG },
                ProgramId,
                out var pda, out var bump);
            return (pda, bump);
        }

        public static (PublicKey pda, byte bump) GetPlayerAuthPDA(PublicKey wallet)
        {
            PublicKey.TryFindProgramAddress(
                new[] { SEED_PLAYER_AUTH, wallet.KeyBytes },
                ProgramId,
                out var pda, out var bump);
            return (pda, bump);
        }

        public static (PublicKey pda, byte bump) GetPlayerProfilePDA(PublicKey wallet)
        {
            PublicKey.TryFindProgramAddress(
                new[] { SEED_PLAYER_PROFILE, wallet.KeyBytes },
                ProgramId,
                out var pda, out var bump);
            return (pda, bump);
        }

        /// <summary>
        /// Derives the PuzzleBoard PDA for a given nonce (puzzles_started_nonce).
        /// nonce should be the current nonce BEFORE incrementing (i.e. the active puzzle index).
        /// </summary>
        public static (PublicKey pda, byte bump) GetPuzzleBoardPDA(PublicKey playerAuthKey, ulong nonce)
        {
            var nonceBytes = System.BitConverter.GetBytes(nonce);
            // Ensure little-endian (Solana standard)
            if (!System.BitConverter.IsLittleEndian)
                System.Array.Reverse(nonceBytes);

            PublicKey.TryFindProgramAddress(
                new[] { SEED_PUZZLE_BOARD, playerAuthKey.KeyBytes, nonceBytes },
                ProgramId,
                out var pda, out var bump);
            return (pda, bump);
        }

        public static (PublicKey pda, byte bump) GetPuzzleStatsPDA(PublicKey playerAuthKey, ulong nonce)
        {
            var nonceBytes = System.BitConverter.GetBytes(nonce);
            if (!System.BitConverter.IsLittleEndian)
                System.Array.Reverse(nonceBytes);

            PublicKey.TryFindProgramAddress(
                new[] { SEED_PUZZLE_STATS, playerAuthKey.KeyBytes, nonceBytes },
                ProgramId,
                out var pda, out var bump);
            return (pda, bump);
        }

        public static (PublicKey pda, byte bump) GetTournamentPDA(ulong tournamentId)
        {
            var idBytes = System.BitConverter.GetBytes(tournamentId);
            if (!System.BitConverter.IsLittleEndian)
                System.Array.Reverse(idBytes);

            PublicKey.TryFindProgramAddress(
                new[] { SEED_TOURNAMENT, idBytes },
                ProgramId,
                out var pda, out var bump);
            return (pda, bump);
        }

        public static (PublicKey pda, byte bump) GetTournamentEntryPDA(
            PublicKey tournamentKey, PublicKey playerKey)
        {
            PublicKey.TryFindProgramAddress(
                new[] { SEED_TOURNAMENT_ENTRY, tournamentKey.KeyBytes, playerKey.KeyBytes },
                ProgramId,
                out var pda, out var bump);
            return (pda, bump);
        }

        // ── Active puzzle helpers ─────────────────────────────────────────────

        /// <summary>
        /// Get the PuzzleBoard PDA for the currently active puzzle.
        /// Uses nonce - 1 (the last started puzzle), matching the Rust seeds.
        /// </summary>
        public static (PublicKey pda, byte bump) GetActivePuzzleBoardPDA(
            PublicKey playerAuthKey, ulong puzzlesStartedNonce)
        {
            ulong activeNonce = puzzlesStartedNonce > 0 ? puzzlesStartedNonce - 1 : 0;
            return GetPuzzleBoardPDA(playerAuthKey, activeNonce);
        }

        public static (PublicKey pda, byte bump) GetActivePuzzleStatsPDA(
            PublicKey playerAuthKey, ulong puzzlesStartedNonce)
        {
            ulong activeNonce = puzzlesStartedNonce > 0 ? puzzlesStartedNonce - 1 : 0;
            return GetPuzzleStatsPDA(playerAuthKey, activeNonce);
        }
    }
}