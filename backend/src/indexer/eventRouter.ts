import { onPuzzleFinalized } from "./handlers/onPuzzleFinalized.js";
import { onPuzzleAbandoned } from "./handlers/onPuzzleAbandoned.js";
import { onTournamentCreated } from "./handlers/onTournamentCreated.js";
import { onTournamentJoined } from "./handlers/onTournamentJoined.js";
import { onTournamentClosed } from "./handlers/onTournamentClosed.js";
import { onPrizeClaimed } from "./handlers/onPrizeClaimed.js";
import { onTournamentResultRecorded } from "./handlers/onTournamentResultRecorded.js";
import { TransactionModel } from "../models/Transaction.js";

/**
 * Routes a single Helius enhanced transaction event to the correct handler.
 * Helius sends the "events" array inside each transaction.
 */
export async function routeEvent(tx: any): Promise<void> {
  if (!tx) return;

  const signature = tx.signature as string;
  const slot = tx.slot as number;
  const blockTime = tx.timestamp as number;
  const instructions = (tx.instructions ?? []) as any[];
  const nativeEvents = (tx.events ?? {}) as Record<string, any>;
  const innerInstructions = (tx.innerInstructions ?? []) as any[];

  // Record every tx in the transactions table
  try {
    const txType = TransactionModel.inferTxType(
      instructions.map((i: any) => i.programInvokeInstruction?.data ?? "")
    );

    const isError = !!tx.transactionError;

    await TransactionModel.record({
      signature,
      tx_type: txType,
      status: isError ? "failed" : "success",
      slot,
      block_time: blockTime,
      fee_lamports: tx.fee,
      compute_units_consumed: tx.computeUnitsConsumed,
      ...(isError ? { error_message: JSON.stringify(tx.transactionError) } : {}),
      raw_meta: {
        accountData: tx.accountData,
        type: tx.type,
        source: tx.source,
      },
    });
  } catch (err: any) {
    console.error("[EventRouter] Failed to record tx:", err.message);
  }

  if (tx.transactionError) {
    console.log(`[EventRouter] Skipping failed tx: ${signature}`);
    return;
  }

  // Helius enhanced events come in tx.events.nft / tx.events.compressed etc.
  // For Anchor programs, events are in the program logs as base64 encoded data.
  // We parse them from the description field or the type field Helius sets.
  const eventType = (tx.type as string) ?? "";

  console.log(`[EventRouter] Routing event: ${eventType} | sig: ${signature?.slice(0, 8)}...`);

  try {
    switch (eventType) {
      case "PUZZLE_FINALIZED":
        await onPuzzleFinalized(tx);
        break;
      case "PUZZLE_ABANDONED":
        await onPuzzleAbandoned(tx);
        break;
      case "TOURNAMENT_CREATED":
        await onTournamentCreated(tx);
        break;
      case "TOURNAMENT_JOINED":
        await onTournamentJoined(tx);
        break;
      case "TOURNAMENT_CLOSED":
        await onTournamentClosed(tx);
        break;
      case "PRIZE_CLAIMED":
        await onPrizeClaimed(tx);
        break;
      case "TOURNAMENT_RESULT_RECORDED":
        await onTournamentResultRecorded(tx);
        break;
      default:
        // Unhandled event types are silently ignored
        console.log(`[EventRouter] Unhandled event type: ${eventType}`);
    }
  } catch (err: any) {
    console.error(`[EventRouter] Handler error for ${eventType}:`, err.message);
    throw err;
  }
}
