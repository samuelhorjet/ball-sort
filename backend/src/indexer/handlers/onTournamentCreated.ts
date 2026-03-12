import { TournamentModel } from "../../models/Tournament.js";
import { ActivityModel } from "../../models/Activity.js";
import { fetchTournamentByAddress } from "../../solana/accounts/fetchTournament.js";
import { WebSocketService } from "../../services/websocket.service.js";
import { WS_EVENTS } from "../../utils/constants.js";

export async function onTournamentCreated(tx: any): Promise<void> {
  const sig = tx.signature as string;

  const alreadyProcessed = await ActivityModel.alreadyProcessed(sig);
  if (alreadyProcessed) return;

  // Tournament PDA is the main writable account in create_tournament
  const accounts = (tx.accountData ?? []) as any[];
  const tournamentAddress = accounts.find((a: any) => a.nativeBalanceChange > 0)?.account
    ?? accounts[1]?.account;

  if (!tournamentAddress) {
    console.warn("[onTournamentCreated] Cannot determine tournament address from tx:", sig);
    return;
  }

  // Fetch authoritative data from chain
  const tournamentData = await fetchTournamentByAddress(tournamentAddress);

  if (!tournamentData) {
    console.warn("[onTournamentCreated] Tournament account not found:", tournamentAddress);
    return;
  }

  await TournamentModel.upsert({
    on_chain_address: tournamentAddress,
    on_chain_id: tournamentData.tournamentId.toString(),
    authority: tournamentData.authority,
    entry_fee: tournamentData.entryFee.toString(),
    prize_pool: tournamentData.prizePool.toString(),
    net_prize_pool: tournamentData.netPrizePool.toString(),
    treasury_fee_bps: tournamentData.treasuryFeeBps,
    difficulty: tournamentData.difficulty,
    start_time: tournamentData.startTime,
    end_time: tournamentData.endTime,
    total_entries: tournamentData.totalEntries,
    total_completers: tournamentData.totalCompleters,
    cumulative_weight: tournamentData.cumulativeWeight.toString(),
    is_closed: tournamentData.isClosed,
  });

  await ActivityModel.log({
    event_type: "tournament_created",
    tournament_address: tournamentAddress,
    tx_signature: sig,
    block_time: tx.timestamp,
    raw_data: {
      difficulty: tournamentData.difficulty,
      entry_fee: tournamentData.entryFee.toString(),
      end_time: tournamentData.endTime,
    },
  });

  WebSocketService.broadcast(WS_EVENTS.TOURNAMENT_CREATED, {
    tournament: tournamentAddress,
    difficulty: tournamentData.difficulty,
    entry_fee: tournamentData.entryFee.toString(),
    end_time: tournamentData.endTime,
  });

  console.log(`[onTournamentCreated] Tournament: ${tournamentAddress.slice(0, 8)}...`);
}
