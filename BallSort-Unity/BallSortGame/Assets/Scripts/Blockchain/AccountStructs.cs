using System;
using Solana.Unity.Wallet;

namespace BallSort.Blockchain
{
    // ── Constants ─────────────────────────────────────────────────────────────
    public static class PuzzleConstants
    {
        public const int MAX_TUBES    = 12;
        public const int MAX_CAPACITY = 10;
        public const int BALLS_LEN    = MAX_TUBES * MAX_CAPACITY; // 120
    }

    // ── PlayerAuth ────────────────────────────────────────────────────────────
    public class PlayerAuth
    {
        public PublicKey Wallet               { get; set; }
        public PublicKey SessionKey           { get; set; }  // null if none
        public long      SessionExpiresAt     { get; set; }
        public bool      HasActivePuzzle      { get; set; }
        public PublicKey ActivePuzzleEntity   { get; set; }  // null if none
        public byte      ActivePuzzleStatus   { get; set; }
        public ulong     TotalPuzzlesSolved   { get; set; }
        public PublicKey SoarPlayerAccount    { get; set; }
        public ulong     PuzzlesStartedNonce  { get; set; }
        public byte[]    VrfRandomness        { get; set; }  // [32]
        public byte      PuzzleNumTubes       { get; set; }
        public byte      PuzzleBallsPerTube   { get; set; }
        public byte      PuzzleDifficulty     { get; set; }
        public byte      Bump                 { get; set; }
    }

    // ── PlayerProfile ─────────────────────────────────────────────────────────
    public class PlayerProfile
    {
        public uint  TotalPuzzlesStarted { get; set; }
        public uint  TotalPuzzlesSolved  { get; set; }
        public ulong BestScoreEasy       { get; set; }
        public ulong BestScoreMedium     { get; set; }
        public ulong BestScoreHard       { get; set; }
    }

    // ── PuzzleBoard ───────────────────────────────────────────────────────────
    public class PuzzleBoard
    {
        public byte   NumTubes     { get; set; }
        public byte   NumColors    { get; set; }
        public byte   MaxCapacity  { get; set; }
        public byte[] Balls        { get; set; }  // [120] flat array
        public byte[] TubeLengths  { get; set; }  // [12]
        public byte[] VrfSeed      { get; set; }  // [32]
        public byte   UndoFrom     { get; set; }
        public byte   UndoTo       { get; set; }
        public byte   UndoBall     { get; set; }
        public bool   HasUndo      { get; set; }

        /// <summary>Get the balls in tube i as a bottom-to-top array.</summary>
        public byte[] GetTubeBalls(int tubeIndex)
        {
            int len  = TubeLengths[tubeIndex];
            var tube = new byte[len];
            int base_ = tubeIndex * PuzzleConstants.MAX_CAPACITY;
            Array.Copy(Balls, base_, tube, 0, len);
            return tube;
        }

        /// <summary>Get the top ball of a tube, or 0 if empty.</summary>
        public byte GetTopBall(int tubeIndex)
        {
            int len = TubeLengths[tubeIndex];
            if (len == 0) return 0;
            return Balls[tubeIndex * PuzzleConstants.MAX_CAPACITY + (len - 1)];
        }

        public bool IsTubeEmpty(int tubeIndex) => TubeLengths[tubeIndex] == 0;
        public bool IsTubeFull(int tubeIndex)  => TubeLengths[tubeIndex] >= MaxCapacity;

        /// <summary>Returns true if a move from→to is legal.
        /// Only restriction: source must have a ball, destination must not be full.
        /// No color-matching rule — any ball can go into any non-full tube.</summary>
        public bool CanMove(int from, int to)
        {
            if (from == to)        return false;
            if (IsTubeEmpty(from)) return false;
            if (IsTubeFull(to))    return false;
            return true;
        }
    }

    // ── PuzzleStats ───────────────────────────────────────────────────────────
    public class PuzzleStats
    {
        public byte   Status      { get; set; }   // 2=active, 3=solved
        public byte   Difficulty  { get; set; }
        public byte   NumTubes    { get; set; }
        public byte   BallsPerTube{ get; set; }
        public uint   MoveCount   { get; set; }
        public uint   UndoCount   { get; set; }
        public long   StartedAt   { get; set; }
        public long   CompletedAt { get; set; }
        public bool   IsSolved    { get; set; }
        public ulong  FinalScore  { get; set; }
    }

    // ── Tournament ────────────────────────────────────────────────────────────
    public class Tournament
    {
        public PublicKey Authority        { get; set; }
        public ulong     EntryFee         { get; set; }
        public ulong     PrizePool        { get; set; }
        public ulong     NetPrizePool     { get; set; }
        public ushort    TreasuryFeeBps   { get; set; }
        public byte      Difficulty       { get; set; }
        public long      StartTime        { get; set; }
        public long      EndTime          { get; set; }
        public uint      TotalEntries     { get; set; }
        public uint      TotalCompleters  { get; set; }
        public bool      IsClosed         { get; set; }
        public ulong     TournamentId     { get; set; }
        public byte      Bump             { get; set; }

        public bool IsOpen(long now) => !IsClosed && now < EndTime;
    }

    // ── TournamentEntry ───────────────────────────────────────────────────────
    public class TournamentEntry
    {
        public PublicKey Tournament       { get; set; }
        public PublicKey Player           { get; set; }
        public PublicKey PuzzleEntity     { get; set; }
        public ulong     EntryDeposit     { get; set; }
        public bool      HasPlayed        { get; set; }
        public bool      Completed        { get; set; }
        public bool      HasClaimed       { get; set; }
        public byte      Bump             { get; set; }
    }

    // ── PuzzleStatus enum (mirrors Rust) ──────────────────────────────────────
    public enum PuzzleStatus : byte
    {
        Initialized = 0,
        BoardReady  = 1,
        Started     = 2,
        Delegated   = 3,
        Solved      = 4,
        Finalized   = 5,
        Abandoned   = 6,
    }
}