import { PuzzleResultModel } from "../../models/PuzzleResult.js";
import { ActivityModel } from "../../models/Activity.js";
import { fetchPuzzleStatsByAddress } from "../../solana/accounts/fetchPuzzleStats.js";
import { WebSocketService } from "../../services/websocket.service.js";
import { WS_EVENTS } from "../../utils/constants.js";

export async function onPuzzleAbandoned(tx: any): Promise<void> {
  const sig = tx.signature as string;

  const playerWallet = tx.feePayer ?? tx.accountData?.[0]?.account ?? null;
  const accounts = (tx.accountData ?? []) as any[];

  // PuzzleStats is usually the 3rd account in abandon_puzzle
  const puzzleStatsAddress = accounts[2]?.account ?? null;

  if (!playerWallet || !puzzleStatsAddress) {
    console.warn("[onPuzzleAbandoned] Missing accounts in tx:", sig);
    return;
  }

  const alreadyProcessed = await ActivityModel.alreadyProcessed(sig);
  if (alreadyProcessed) return;

  const statsAccount = await fetchPuzzleStatsByAddress(puzzleStatsAddress);

  if (!statsAccount) {
    console.warn("[onPuzzleAbandoned] PuzzleStats account not found:", puzzleStatsAddress);
    return;
  }

  const exists = await PuzzleResultModel.existsByStatsAccount(puzzleStatsAddress);
  if (!exists) {
    await PuzzleResultModel.create({
      player_wallet: playerWallet,
      puzzle_board_pubkey: puzzleStatsAddress, // may not have board addr, use stats as placeholder
      puzzle_stats_pubkey: puzzleStatsAddress,
      difficulty: statsAccount.difficulty,
      num_tubes: statsAccount.numTubes,
      balls_per_tube: statsAccount.ballsPerTube,
      move_count: statsAccount.moveCount,
      undo_count: statsAccount.undoCount,
      elapsed_secs: 0,
      final_score: 0,
      is_abandoned: true,
      is_solved: false,
      tx_signature: sig,
    });
  }

  await ActivityModel.log({
    event_type: "puzzle_abandoned",
    player_wallet: playerWallet,
    puzzle_stats_pubkey: puzzleStatsAddress,
    tx_signature: sig,
    block_time: tx.timestamp,
    raw_data: {
      difficulty: statsAccount.difficulty,
      move_count: statsAccount.moveCount,
    },
  });

  WebSocketService.broadcast(WS_EVENTS.PUZZLE_ABANDONED, {
    player: playerWallet,
    difficulty: statsAccount.difficulty,
    move_count: statsAccount.moveCount,
  });

  console.log(`[onPuzzleAbandoned] Processed: ${playerWallet.slice(0, 8)}...`);
}
