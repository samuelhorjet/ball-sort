import { TournamentEntryModel } from "../../models/TournamentEntry.js";
import { ActivityModel } from "../../models/Activity.js";
import { WebSocketService } from "../../services/websocket.service.js";
import { WS_EVENTS } from "../../utils/constants.js";

export async function onPrizeClaimed(tx: any): Promise<void> {
  const sig = tx.signature as string;

  const alreadyProcessed = await ActivityModel.alreadyProcessed(sig);
  if (alreadyProcessed) return;

  const playerWallet = tx.feePayer ?? tx.accountData?.[0]?.account;
  const accounts = (tx.accountData ?? []) as any[];

  // Accounts in claim_prize: player, tournament, tournament_entry, system_program
  const tournamentAddress = accounts[1]?.account ?? null;

  // Determine prize amount from native SOL transfer
  const nativeTransfers = (tx.nativeTransfers ?? []) as any[];
  const prizeTransfer = nativeTransfers.find(
    (t: any) => t.toUserAccount === playerWallet
  );
  const prizeAmount = prizeTransfer?.amount ?? 0;

  if (!playerWallet || !tournamentAddress) {
    console.warn("[onPrizeClaimed] Missing accounts:", sig);
    return;
  }

  await TournamentEntryModel.markClaimed(
    tournamentAddress,
    playerWallet,
    prizeAmount.toString()
  );

  await ActivityModel.log({
    event_type: "prize_claimed",
    player_wallet: playerWallet,
    tournament_address: tournamentAddress,
    tx_signature: sig,
    block_time: tx.timestamp,
    raw_data: { prize_amount: prizeAmount },
  });

  WebSocketService.broadcast(WS_EVENTS.PRIZE_CLAIMED, {
    tournament: tournamentAddress,
    player: playerWallet,
    amount: prizeAmount,
  });

  console.log(`[onPrizeClaimed] Player ${playerWallet.slice(0, 8)}... claimed ${prizeAmount} lamports`);
}
