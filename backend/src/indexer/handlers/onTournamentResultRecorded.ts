import { TournamentEntryModel } from "../../models/TournamentEntry.js";
import { TournamentModel } from "../../models/Tournament.js";
import { ActivityModel } from "../../models/Activity.js";
import { fetchTournamentEntry } from "../../solana/accounts/fetchTournamentEntry.js";
import { fetchTournamentByAddress } from "../../solana/accounts/fetchTournament.js";

export async function onTournamentResultRecorded(tx: any): Promise<void> {
  const sig = tx.signature as string;

  const alreadyProcessed = await ActivityModel.alreadyProcessed(sig);
  if (alreadyProcessed) return;

  const playerWallet = tx.feePayer ?? tx.accountData?.[0]?.account;
  const accounts = (tx.accountData ?? []) as any[];

  // Accounts in record_tournament_result: player, tournament, tournament_entry
  const tournamentAddress = accounts[1]?.account ?? null;

  if (!playerWallet || !tournamentAddress) {
    console.warn("[onTournamentResultRecorded] Missing accounts:", sig);
    return;
  }

  // Fetch entry from chain to get weight, elapsed, moves
  const entryData = await fetchTournamentEntry(tournamentAddress, playerWallet);
  const tournamentData = await fetchTournamentByAddress(tournamentAddress);

  if (!entryData) {
    console.warn("[onTournamentResultRecorded] Entry not found on chain:", sig);
    return;
  }

  await TournamentEntryModel.markCompleted(
    tournamentAddress,
    playerWallet,
    entryData.parimutuelWeight.toString().length > 0 ? 0 : 0, // elapsed not in entry; estimate from tx
    0,
    entryData.parimutuelWeight.toString()
  );

  if (tournamentData) {
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
  }

  await ActivityModel.log({
    event_type: "tournament_result_recorded",
    player_wallet: playerWallet,
    tournament_address: tournamentAddress,
    tx_signature: sig,
    block_time: tx.timestamp,
    raw_data: {
      parimutuel_weight: entryData.parimutuelWeight.toString(),
      completed: entryData.completed,
    },
  });

  console.log(`[onTournamentResultRecorded] Player ${playerWallet.slice(0, 8)}... recorded result`);
}
