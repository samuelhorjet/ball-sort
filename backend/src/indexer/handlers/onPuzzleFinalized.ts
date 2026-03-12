import { PuzzleResultModel } from "../../models/PuzzleResult.js";
import { PlayerModel } from "../../models/Player.js";
import { ActivityModel } from "../../models/Activity.js";
import { fetchPuzzleStatsByAddress } from "../../solana/accounts/fetchPuzzleStats.js";
import { WebSocketService } from "../../services/websocket.service.js";
import { WS_EVENTS } from "../../utils/constants.js";

/**
 * Handles the PuzzleFinalized event from Helius.
 * The event payload contains: player, puzzle_board, puzzle_stats, move_count,
 * undo_count, difficulty, timestamp.
 *
 * We read the PuzzleStats account directly from chain for authoritative data.
 */
export async function onPuzzleFinalized(tx: any): Promise<void> {
  const sig = tx.signature as string;

  // Extract accounts involved from the Helius account data
  const accountData = (tx.accountData ?? []) as any[];

  // The puzzle_stats account is the one with the PuzzleStats discriminator
  // We use the accounts from the instruction
  const puzzleStatsAddress = extractAccountFromTx(tx, "puzzle_stats");
  const puzzleBoardAddress = extractAccountFromTx(tx, "puzzle_board");
  const playerWallet = extractSignerFromTx(tx);

  if (!puzzleStatsAddress || !playerWallet) {
    console.warn("[onPuzzleFinalized] Missing puzzle_stats or player in tx:", sig);
    return;
  }

  // Idempotency check
  const alreadyProcessed = await ActivityModel.alreadyProcessed(sig);
  if (alreadyProcessed) return;

  // Read authoritative data from chain
  const statsAccount = await fetchPuzzleStatsByAddress(puzzleStatsAddress);

  if (!statsAccount) {
    console.warn("[onPuzzleFinalized] PuzzleStats account not found:", puzzleStatsAddress);
    return;
  }

  const elapsedSecs =
    statsAccount.completedAt > 0
      ? statsAccount.completedAt - statsAccount.startedAt
      : 0;

  // Check for duplicate puzzle result
  const exists = await PuzzleResultModel.existsByStatsAccount(puzzleStatsAddress);

  if (!exists) {
    await PuzzleResultModel.create({
      player_wallet: playerWallet,
      puzzle_board_pubkey: puzzleBoardAddress ?? puzzleStatsAddress,
      puzzle_stats_pubkey: puzzleStatsAddress,
      difficulty: statsAccount.difficulty,
      num_tubes: statsAccount.numTubes,
      balls_per_tube: statsAccount.ballsPerTube,
      move_count: statsAccount.moveCount,
      undo_count: statsAccount.undoCount,
      elapsed_secs: elapsedSecs,
      final_score: Number(statsAccount.finalScore),
      is_abandoned: false,
      is_solved: statsAccount.isSolved,
      tx_signature: sig,
    });

    if (statsAccount.isSolved) {
      await PlayerModel.incrementSolved(playerWallet, Number(statsAccount.finalScore));
    }
  }

  // Log the activity
  await ActivityModel.log({
    event_type: "puzzle_finalized",
    player_wallet: playerWallet,
    puzzle_board_pubkey: puzzleBoardAddress ?? "",
    puzzle_stats_pubkey: puzzleStatsAddress,
    tx_signature: sig,
    block_time: tx.timestamp,
    raw_data: {
      difficulty: statsAccount.difficulty,
      move_count: statsAccount.moveCount,
      undo_count: statsAccount.undoCount,
      final_score: statsAccount.finalScore.toString(),
      is_solved: statsAccount.isSolved,
    },
  });

  // Broadcast via WebSocket
  if (statsAccount.isSolved) {
    WebSocketService.broadcast(WS_EVENTS.PUZZLE_SOLVED, {
      player: playerWallet,
      difficulty: statsAccount.difficulty,
      score: Number(statsAccount.finalScore),
      move_count: statsAccount.moveCount,
      elapsed_secs: elapsedSecs,
    });
  }

  console.log(`[onPuzzleFinalized] Processed: ${playerWallet.slice(0, 8)}... score=${statsAccount.finalScore}`);
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

function extractAccountFromTx(tx: any, label: string): string | null {
  // Helius enhanced tx includes accountData array; find by account label
  const accounts = tx.accountData ?? [];
  // In practice we look at the accounts array in the instruction
  const instructions = tx.instructions ?? [];
  for (const ix of instructions) {
    if (ix.accounts) {
      const acc = ix.accounts.find((a: any) =>
        typeof a === "object" && a.label === label
      );
      if (acc?.pubkey) return acc.pubkey;
    }
  }
  // Fallback: look in nativeTransfers or accountData
  return accounts[1]?.account ?? null;
}

function extractSignerFromTx(tx: any): string | null {
  // The fee payer / first signer is the player's wallet
  return tx.feePayer ?? tx.accountData?.[0]?.account ?? null;
}
