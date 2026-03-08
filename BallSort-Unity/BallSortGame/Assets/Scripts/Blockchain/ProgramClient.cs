using System.Collections.Generic;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Programs;
using BallSort.Blockchain;

namespace BallSort.Blockchain
{
    /// <summary>
    /// Builds Anchor instructions for the ball_sort program.
    /// Generated from ball_sort.json IDL — mirrors the TypeScript Anchor client.
    ///
    /// Each method returns a TransactionInstruction ready to be added to a Transaction.
    /// </summary>
    public static class ProgramClient
    {
        public static readonly PublicKey ProgramId  = PDAHelper.ProgramId;
        public static readonly PublicKey SystemProg = new PublicKey("11111111111111111111111111111111");

        // ── Well-known program / sysvar addresses ─────────────────────────────
        /// <summary>Switchboard VRF program (from IDL).</summary>
        public static readonly PublicKey VrfProgram =
            new PublicKey("Vrf1RNUjXmQGjmQrQLvJHs9SNkvDJEsRVFPkfSQUwGz");

        /// <summary>Solana SlotHashes sysvar (from IDL).</summary>
        public static readonly PublicKey SlotHashesSysvar =
            new PublicKey("SysvarS1otHashes111111111111111111111111111");

        /// <summary>Oracle queue address used by init_puzzle (hardcoded in IDL).</summary>
        public static readonly PublicKey OracleQueue =
            new PublicKey("Cuj97ggrhhidhbu39TijNVqE74xvKJ69gDervRUXAxGh");

        private static readonly byte[] DISC_CREATE_PLAYER_AUTH       = new byte[] { 121, 167, 229, 196, 32, 81, 30, 142 };
        private static readonly byte[] DISC_OPEN_SESSION             = new byte[] { 130, 54, 124, 7, 236, 20, 104, 104 };
        private static readonly byte[] DISC_CLOSE_SESSION            = new byte[] { 68, 114, 178, 140, 222, 38, 248, 211 };
        private static readonly byte[] DISC_INIT_PUZZLE              = new byte[] { 115, 166, 68, 22, 105, 35, 115, 146 };
        private static readonly byte[] DISC_START_PUZZLE             = new byte[] { 185, 248, 132, 48, 101, 15, 200, 249 };
        private static readonly byte[] DISC_APPLY_MOVE               = new byte[] { 196, 116, 174, 213, 239, 192, 203, 47 };
        private static readonly byte[] DISC_APPLY_UNDO               = new byte[] { 67, 111, 241, 156, 9, 0, 239, 220 };
        private static readonly byte[] DISC_ABANDON_PUZZLE           = new byte[] { 60, 221, 92, 169, 114, 215, 1, 141 };
        private static readonly byte[] DISC_FINALIZE_PUZZLE          = new byte[] { 122, 117, 70, 166, 16, 241, 40, 27 };
        private static readonly byte[] DISC_CREATE_PUZZLE_PERMS      = new byte[] { 35, 188, 175, 155, 161, 55, 220, 145 };
        private static readonly byte[] DISC_DELEGATE_PUZZLE_PERMS    = new byte[] { 96, 192, 209, 47, 173, 239, 126, 35 };
        private static readonly byte[] DISC_DELEGATE_PUZZLE          = new byte[] { 236, 63, 240, 113, 3, 153, 123, 218 };
        private static readonly byte[] DISC_UNDELEGATE_PUZZLE        = new byte[] { 14, 237, 88, 59, 241, 22, 86, 91 };
        private static readonly byte[] DISC_JOIN_TOURNAMENT          = new byte[] { 77, 21, 212, 206, 77, 82, 124, 31 };
        private static readonly byte[] DISC_RECORD_TOURNAMENT_RESULT = new byte[] { 70, 158, 217, 88, 63, 68, 232, 254 };
        private static readonly byte[] DISC_CLOSE_TOURNAMENT         = new byte[] { 14, 80, 54, 9, 221, 239, 201, 35 };
        private static readonly byte[] DISC_CLAIM_PRIZE              = new byte[] { 157, 233, 139, 121, 246, 62, 234, 235 };
        private static readonly byte[] DISC_CLAIM_REFUND             = new byte[] { 15, 16, 30, 161, 255, 228, 97, 60 };

        // ── Setup instructions ────────────────────────────────────────────────

        public static TransactionInstruction BuildCreatePlayerAuthIx(
            PublicKey player, PublicKey playerAuth, PublicKey gameConfig, PublicKey playerProfile)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(player,        isSigner: true),
                    AccountMeta.Writable(playerAuth,    isSigner: false),
                    AccountMeta.ReadOnly(gameConfig,    isSigner: false),
                    AccountMeta.Writable(playerProfile, isSigner: false),
                    AccountMeta.ReadOnly(SystemProg,    isSigner: false),
                },
                Data = DISC_CREATE_PLAYER_AUTH
            };
        }

        public static TransactionInstruction BuildOpenSessionIx(
            PublicKey player, PublicKey playerAuth,
            PublicKey sessionKey, uint expiresInSecs)
        {
            var data = new List<byte>(DISC_OPEN_SESSION);
            data.AddRange(sessionKey.KeyBytes);          // Pubkey (32 bytes)
            data.AddRange(System.BitConverter.GetBytes(expiresInSecs)); // u32 (4 bytes)

            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(player,     isSigner: true),
                    AccountMeta.Writable(playerAuth, isSigner: false),
                },
                Data = data.ToArray()
            };
        }

        public static TransactionInstruction BuildCloseSessionIx(
            PublicKey player, PublicKey playerAuth)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.ReadOnly(player,     isSigner: true),
                    AccountMeta.Writable(playerAuth, isSigner: false),
                },
                Data = DISC_CLOSE_SESSION
            };
        }

        // ── Puzzle instructions ───────────────────────────────────────────────

        public static TransactionInstruction BuildInitPuzzleIx(
            PublicKey signer, PublicKey playerAuth, PublicKey gameConfig,
            PublicKey puzzleBoard, PublicKey puzzleStats,
            byte numTubes, byte ballsPerTube, byte difficulty, byte gameMode)
        {
            var data = new List<byte>(DISC_INIT_PUZZLE);
            data.Add(numTubes);
            data.Add(ballsPerTube);
            data.Add(difficulty);
            data.Add(gameMode);

            // Derive the program_identity PDA (seed = "identity", program = ProgramId)
            var (programIdentity, _) = PDAHelper.GetProgramIdentityPDA();

            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    // IDL accounts [0-9] — order MUST match IDL exactly
                    AccountMeta.Writable(signer,           isSigner: true),   // [0] signer
                    AccountMeta.Writable(playerAuth,       isSigner: false),  // [1] player_auth
                    AccountMeta.ReadOnly(gameConfig,       isSigner: false),  // [2] game_config
                    AccountMeta.Writable(puzzleBoard,      isSigner: false),  // [3] puzzle_board
                    AccountMeta.Writable(puzzleStats,      isSigner: false),  // [4] puzzle_stats
                    AccountMeta.Writable(OracleQueue,      isSigner: false),  // [5] oracle_queue (IDL-hardcoded address)
                    AccountMeta.ReadOnly(SystemProg,       isSigner: false),  // [6] system_program
                    AccountMeta.ReadOnly(programIdentity,  isSigner: false),  // [7] program_identity (PDA seed="identity")
                    AccountMeta.ReadOnly(VrfProgram,       isSigner: false),  // [8] vrf_program
                    AccountMeta.ReadOnly(SlotHashesSysvar, isSigner: false),  // [9] slot_hashes sysvar
                },
                Data = data.ToArray()
            };
        }

        public static TransactionInstruction BuildStartPuzzleIx(
            PublicKey signer, PublicKey playerAuth, PublicKey gameConfig,
            PublicKey puzzleBoard, PublicKey puzzleStats)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.ReadOnly(signer,      isSigner: true),
                    AccountMeta.Writable(playerAuth,  isSigner: false),
                    AccountMeta.ReadOnly(gameConfig,  isSigner: false),
                    AccountMeta.Writable(puzzleBoard, isSigner: false),
                    AccountMeta.Writable(puzzleStats, isSigner: false),
                },
                Data = DISC_START_PUZZLE
            };
        }

        public static TransactionInstruction BuildApplyMoveIx(
            PublicKey signer, PublicKey playerAuth, PublicKey gameConfig,
            PublicKey puzzleBoard, PublicKey puzzleStats,
            byte fromTube, byte toTube)
        {
            var data = new List<byte>(DISC_APPLY_MOVE);
            data.Add(fromTube);
            data.Add(toTube);

            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.ReadOnly(signer,      isSigner: true),
                    AccountMeta.ReadOnly(playerAuth,  isSigner: false),
                    AccountMeta.ReadOnly(gameConfig,  isSigner: false),
                    AccountMeta.Writable(puzzleBoard, isSigner: false),
                    AccountMeta.Writable(puzzleStats, isSigner: false),
                },
                Data = data.ToArray()
            };
        }

        public static TransactionInstruction BuildApplyUndoIx(
            PublicKey signer, PublicKey playerAuth, PublicKey gameConfig,
            PublicKey puzzleBoard, PublicKey puzzleStats)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.ReadOnly(signer,      isSigner: true),
                    AccountMeta.ReadOnly(playerAuth,  isSigner: false),
                    AccountMeta.ReadOnly(gameConfig,  isSigner: false),
                    AccountMeta.Writable(puzzleBoard, isSigner: false),
                    AccountMeta.Writable(puzzleStats, isSigner: false),
                },
                Data = DISC_APPLY_UNDO
            };
        }

        public static TransactionInstruction BuildAbandonPuzzleIx(
            PublicKey signer, PublicKey playerAuth, PublicKey gameConfig)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(signer,     isSigner: true),
                    AccountMeta.Writable(playerAuth, isSigner: false),
                    AccountMeta.ReadOnly(gameConfig, isSigner: false),
                },
                Data = DISC_ABANDON_PUZZLE
            };
        }

        public static TransactionInstruction BuildFinalizePuzzleIx(
            PublicKey signer, PublicKey playerAuth,
            PublicKey gameConfig, PublicKey puzzleStats)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(signer,      isSigner: true),
                    AccountMeta.Writable(playerAuth,  isSigner: false),
                    AccountMeta.ReadOnly(gameConfig,  isSigner: false),
                    AccountMeta.ReadOnly(puzzleStats, isSigner: false),
                },
                Data = DISC_FINALIZE_PUZZLE
            };
        }

        // ── Ephemeral Rollup (MagicBlock) Instructions ────────────────────────

        public static TransactionInstruction BuildCreatePuzzlePermissionsIx(
            PublicKey payer, PublicKey playerAuth, PublicKey gameConfig,
            PublicKey puzzleBoard, PublicKey puzzleStats,
            PublicKey boardPermission, PublicKey statsPermission,
            PublicKey permissionProgram)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(payer, true),
                    AccountMeta.ReadOnly(playerAuth, false),
                    AccountMeta.ReadOnly(gameConfig, false),
                    AccountMeta.Writable(puzzleBoard, false),
                    AccountMeta.Writable(puzzleStats, false),
                    AccountMeta.Writable(boardPermission, false),
                    AccountMeta.Writable(statsPermission, false),
                    AccountMeta.ReadOnly(permissionProgram, false),
                    AccountMeta.ReadOnly(SystemProg, false)
                },
                Data = DISC_CREATE_PUZZLE_PERMS
            };
        }

        public static TransactionInstruction BuildDelegatePuzzlePermissionsIx(
            PublicKey payer, PublicKey playerAuth, PublicKey gameConfig,
            PublicKey puzzleBoard, PublicKey puzzleStats,
            PublicKey boardPermission, PublicKey statsPermission,
            PublicKey permissionProgram, PublicKey delegationProgram,
            PublicKey boardDelegBuffer, PublicKey boardDelegRecord, PublicKey boardDelegMeta,
            PublicKey statsDelegBuffer, PublicKey statsDelegRecord, PublicKey statsDelegMeta,
            PublicKey validator)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(payer, true),
                    AccountMeta.ReadOnly(playerAuth, false),
                    AccountMeta.ReadOnly(gameConfig, false),
                    AccountMeta.Writable(puzzleBoard, false),
                    AccountMeta.Writable(puzzleStats, false),
                    AccountMeta.Writable(boardPermission, false),
                    AccountMeta.Writable(statsPermission, false),
                    AccountMeta.ReadOnly(permissionProgram, false),
                    AccountMeta.ReadOnly(delegationProgram, false),
                    AccountMeta.Writable(boardDelegBuffer, false),
                    AccountMeta.Writable(boardDelegRecord, false),
                    AccountMeta.Writable(boardDelegMeta, false),
                    AccountMeta.Writable(statsDelegBuffer, false),
                    AccountMeta.Writable(statsDelegRecord, false),
                    AccountMeta.Writable(statsDelegMeta, false),
                    AccountMeta.ReadOnly(validator, false),
                    AccountMeta.ReadOnly(SystemProg, false)
                },
                Data = DISC_DELEGATE_PUZZLE_PERMS
            };
        }

        public static TransactionInstruction BuildDelegatePuzzleIx(
            PublicKey payer, PublicKey playerAuth, PublicKey gameConfig,
            PublicKey bufferBoard, PublicKey recordBoard, PublicKey metaBoard, PublicKey puzzleBoard,
            PublicKey bufferStats, PublicKey recordStats, PublicKey metaStats, PublicKey puzzleStats,
            PublicKey validator, PublicKey ownerProgram, PublicKey delegationProgram)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(payer, true),
                    AccountMeta.ReadOnly(playerAuth, false),
                    AccountMeta.ReadOnly(gameConfig, false),
                    AccountMeta.Writable(bufferBoard, false),
                    AccountMeta.Writable(recordBoard, false),
                    AccountMeta.Writable(metaBoard, false),
                    AccountMeta.Writable(puzzleBoard, false),
                    AccountMeta.Writable(bufferStats, false),
                    AccountMeta.Writable(recordStats, false),
                    AccountMeta.Writable(metaStats, false),
                    AccountMeta.Writable(puzzleStats, false),
                    AccountMeta.ReadOnly(validator, false),
                    AccountMeta.ReadOnly(ownerProgram, false),
                    AccountMeta.ReadOnly(delegationProgram, false),
                    AccountMeta.ReadOnly(SystemProg, false)
                },
                Data = DISC_DELEGATE_PUZZLE
            };
        }

        public static TransactionInstruction BuildUndelegatePuzzleIx(
            PublicKey payer, PublicKey playerAuth,
            PublicKey gameConfig, PublicKey puzzleBoard, PublicKey puzzleStats,
            PublicKey magicProgram, PublicKey magicContext)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(payer,        isSigner: true),
                    AccountMeta.ReadOnly(playerAuth,   isSigner: false),
                    AccountMeta.ReadOnly(gameConfig,   isSigner: false),
                    AccountMeta.Writable(puzzleBoard,  isSigner: false),
                    AccountMeta.Writable(puzzleStats,  isSigner: false),
                    AccountMeta.ReadOnly(magicProgram, isSigner: false),
                    AccountMeta.Writable(magicContext, isSigner: false)
                },
                Data = DISC_UNDELEGATE_PUZZLE
            };
        }

        // ── Tournament instructions ───────────────────────────────────────────

        public static TransactionInstruction BuildJoinTournamentIx(
            PublicKey player, PublicKey playerAuth,
            PublicKey tournament, PublicKey tournamentEntry)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    // IDL accounts [0-4] — exact order
                    AccountMeta.Writable(player,          isSigner: true),   // [0] player
                    AccountMeta.ReadOnly(playerAuth,      isSigner: false),  // [1] player_auth
                    AccountMeta.Writable(tournament,      isSigner: false),  // [2] tournament
                    AccountMeta.Writable(tournamentEntry, isSigner: false),  // [3] tournament_entry
                    AccountMeta.ReadOnly(SystemProg,      isSigner: false),  // [4] system_program
                },
                Data = DISC_JOIN_TOURNAMENT
            };
        }

        public static TransactionInstruction BuildRecordTournamentResultIx(
            PublicKey player, PublicKey tournament, PublicKey tournamentEntry,
            ulong elapsedSecs, uint moveCount)
        {
            var data = new List<byte>(DISC_RECORD_TOURNAMENT_RESULT);
            data.AddRange(System.BitConverter.GetBytes(elapsedSecs));
            data.AddRange(System.BitConverter.GetBytes(moveCount));

            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.ReadOnly(player,          isSigner: true),
                    AccountMeta.Writable(tournament,      isSigner: false),
                    AccountMeta.Writable(tournamentEntry, isSigner: false),
                },
                Data = data.ToArray()
            };
        }

        public static TransactionInstruction BuildCloseTournamentIx(
            PublicKey authority, PublicKey tournament, PublicKey gameConfig, PublicKey treasury)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    // IDL: authority writable=False — ReadOnly, not Writable
                    AccountMeta.ReadOnly(authority,  isSigner: true),   // [0] authority
                    AccountMeta.Writable(tournament, isSigner: false),  // [1] tournament
                    AccountMeta.ReadOnly(gameConfig, isSigner: false),  // [2] game_config
                    AccountMeta.Writable(treasury,   isSigner: false),  // [3] treasury
                    AccountMeta.ReadOnly(SystemProg, isSigner: false),  // [4] system_program
                },
                Data = DISC_CLOSE_TOURNAMENT
            };
        }

        public static TransactionInstruction BuildClaimPrizeIx(
            PublicKey player, PublicKey tournament, PublicKey tournamentEntry)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(player,          isSigner: true),
                    AccountMeta.Writable(tournament,      isSigner: false),
                    AccountMeta.Writable(tournamentEntry, isSigner: false),
                    AccountMeta.ReadOnly(SystemProg,      isSigner: false),
                },
                Data = DISC_CLAIM_PRIZE
            };
        }

        public static TransactionInstruction BuildClaimRefundIx(
            PublicKey player, PublicKey tournament, PublicKey tournamentEntry)
        {
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = new List<AccountMeta>
                {
                    AccountMeta.Writable(player,          isSigner: true),
                    AccountMeta.Writable(tournament,      isSigner: false),
                    AccountMeta.Writable(tournamentEntry, isSigner: false),
                    AccountMeta.ReadOnly(SystemProg,      isSigner: false),
                },
                Data = DISC_CLAIM_REFUND
            };
        }
    }
}