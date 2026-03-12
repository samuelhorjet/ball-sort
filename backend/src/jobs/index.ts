import { startSyncProtocol } from "./syncProtocol.js";
import { startSyncLeaderboard } from "./syncLeaderboard.js";
import { startSyncActiveTournaments } from "./syncActiveTournaments.js";
import { startSyncPuzzleStats } from "./syncPuzzleStats.js";

export function startAllJobs(): void {
  console.log("\n🕐 Starting cron jobs...");
  startSyncProtocol();
  startSyncLeaderboard();
  startSyncActiveTournaments();
  startSyncPuzzleStats();
  console.log("✅ All cron jobs registered.\n");
}
