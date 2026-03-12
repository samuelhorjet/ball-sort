import { TournamentModel } from "../../models/Tournament.js";
import { ActivityModel } from "../../models/Activity.js";
import { fetchTournamentByAddress } from "../../solana/accounts/fetchTournament.js";
import { WebSocketService } from "../../services/websocket.service.js";
import { WS_EVENTS } from "../../utils/constants.js";

export async function onTournamentClosed(tx: any): Promise<void> {
  const sig = tx.signature as string;

  const alreadyProcessed = await ActivityModel.alreadyProcessed(sig);
  if (alreadyProcessed) return;

  const accounts = (tx.accountData ?? []) as any[];
  // Tournament is the first writable account in close_tournament (authority, tournament, game_config, treasury, system)
  const tournamentAddress = accounts[1]?.account ?? accounts[0]?.account;

  if (!tournamentAddress) {
    console.warn("[onTournamentClosed] Cannot find tournament address:", sig);
    return;
  }

  const tournamentData = await fetchTournamentByAddress(tournamentAddress);

  if (!tournamentData) {
    // Tournament may have been closed and data cleared; mark it closed using what we have
    await TournamentModel.markClosed(tournamentAddress, 0, 0, "0");
  } else {
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
      is_closed: true,
    });
  }

  await ActivityModel.log({
    event_type: "tournament_closed",
    tournament_address: tournamentAddress,
    tx_signature: sig,
    block_time: tx.timestamp,
    raw_data: {
      total_entries: tournamentData?.totalEntries,
      total_completers: tournamentData?.totalCompleters,
      prize_pool: tournamentData?.prizePool.toString(),
    },
  });

  WebSocketService.broadcast(WS_EVENTS.TOURNAMENT_CLOSED, {
    tournament: tournamentAddress,
    total_entries: tournamentData?.totalEntries,
    prize_pool: tournamentData?.prizePool.toString(),
  });

  console.log(`[onTournamentClosed] Tournament: ${tournamentAddress.slice(0, 8)}...`);
}
