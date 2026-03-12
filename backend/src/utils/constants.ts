import { PublicKey } from "@solana/web3.js";

// ─── Program ────────────────────────────────────────────────────────────────
export const PROGRAM_ID = new PublicKey(
  "5f83UfHKwf9V9apbsYQGajbgLAtToA1YAimZeSreJz7D"
);

// ─── PDA Seeds ───────────────────────────────────────────────────────────────
export const SEEDS = {
  GAME_CONFIG: Buffer.from("game_config"),
  PLAYER_AUTH: Buffer.from("player_auth"),
  PUZZLE_BOARD: Buffer.from("puzzle_board"),
  PUZZLE_STATS: Buffer.from("puzzle_stats"),
  TOURNAMENT: Buffer.from("tournament"),
  TOURNAMENT_ENTRY: Buffer.from("tournament_entry"),
} as const;

// ─── Puzzle Status ────────────────────────────────────────────────────────────
export const PUZZLE_STATUS = {
  INITIALIZED: 0,
  BOARD_READY: 1,
  STARTED: 2,
  SOLVED: 3,
  FINALIZED: 4,
  ABANDONED: 5,
} as const;

export type PuzzleStatus = (typeof PUZZLE_STATUS)[keyof typeof PUZZLE_STATUS];

// ─── Difficulty ───────────────────────────────────────────────────────────────
export const DIFFICULTY = {
  0: "easy",
  1: "medium",
  2: "hard",
} as const;

export type DifficultyLevel = 0 | 1 | 2;

export function difficultyLabel(d: number): string {
  return DIFFICULTY[d as DifficultyLevel] ?? "unknown";
}

// ─── Score Constants (mirrors Rust program) ──────────────────────────────────
export const BASE_POINTS = 1_000;
export const SPEED_BONUS_WINDOW_SECS = 300; // 5 min
export const UNDO_PENALTY = 50;
export const BPS_DENOMINATOR = 10_000;
export const MAX_TREASURY_FEE_BPS = 2_000; // 20%

// ─── Cron Schedules ──────────────────────────────────────────────────────────
export const CRON = {
  SYNC_PROTOCOL: "*/5 * * * *",           // every 5 min
  SYNC_LEADERBOARD: "*/2 * * * *",        // every 2 min
  SYNC_TOURNAMENTS: "*/1 * * * *",        // every 1 min
  SYNC_PUZZLE_STATS: "*/30 * * * * *",    // every 30 sec
} as const;

// ─── Helius Event Types ───────────────────────────────────────────────────────
export const HELIUS_EVENT_TYPES = {
  PUZZLE_FINALIZED: "PuzzleFinalized",
  PUZZLE_ABANDONED: "PuzzleAbandoned",
  PUZZLE_INITIALIZED: "PuzzleInitialized",
  PUZZLE_STARTED: "PuzzleStarted",
  TOURNAMENT_CREATED: "TournamentCreated",
  TOURNAMENT_JOINED: "TournamentJoined",
  TOURNAMENT_CLOSED: "TournamentClosed",
  TOURNAMENT_RESULT_RECORDED: "TournamentResultRecorded",
  PRIZE_CLAIMED: "PrizeClaimed",
} as const;

// ─── WebSocket Event Types ────────────────────────────────────────────────────
export const WS_EVENTS = {
  PUZZLE_SOLVED: "puzzle:solved",
  PUZZLE_ABANDONED: "puzzle:abandoned",
  TOURNAMENT_CREATED: "tournament:created",
  TOURNAMENT_JOINED: "tournament:joined",
  TOURNAMENT_CLOSED: "tournament:closed",
  LEADERBOARD_UPDATED: "leaderboard:updated",
  PRIZE_CLAIMED: "prize:claimed",
} as const;
