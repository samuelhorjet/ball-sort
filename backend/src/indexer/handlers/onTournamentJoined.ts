import { TournamentModel } from "../../models/Tournament.js";
import { TournamentEntryModel } from "../../models/TournamentEntry.js";
import { ActivityModel } from "../../models/Activity.js";
import { fetchTournamentByAddress } from "../../solana/accounts/fetchTournament.js";
import { fetchTournamentEntry } from "../../solana/accounts/fetchTournamentEntry.js";
import { WebSocketService } from "../../services/websocket.service.js";
import { WS_EVENTS } from "../../utils/constants.js";

export async function onTournamentJoined(tx: any): Promise<void> {
  const sig = tx.signature as string;

  const alreadyProcessed = await ActivityModel.alreadyProcessed(sig);
  if (alreadyProcessed) return;

  const playerWallet = tx.feePayer ?? tx.accountData?.[0]?.account;
  const accounts = (tx.accountData ?? []) as any[];

  // Tournament is the 3rd account in join_tournament (player, player_auth, tournament, entry, system_program)
  const tournamentAddress = accounts[2]?.account ?? null;

  if (!playerWallet || !tournamentAddress) {
    console.warn("[onTournamentJoined] Missing accounts:", sig);
    return;
  }

  // Fetch on-chain entry data
  const entryData = await fetchTournamentEntry(tournamentAddress, playerWallet);
  const tournamentData = await fetchTournamentByAddress(tournamentAddress);

  if (!entryData || !tournamentData) {
    console.warn("[onTournamentJoined] On-chain data not found:", sig);
    return;
  }

  // Ensure tournament exists in DB
  const dbTournament = await TournamentModel.findByOnChainAddress(tournamentAddress);

  if (dbTournament) {
    // Upsert the entry
    await TournamentEntryModel.upsert({
      tournament_id: dbTournament.id,
      on_chain_entry_address: entryData.address,
      tournament_address: tournamentAddress,
      player_wallet: playerWallet,
      entry_deposit: entryData.entryDeposit.toString(),
      parimutuel_weight: entryData.parimutuelWeight.toString(),
      completed: entryData.completed,
      has_claimed: entryData.hasClaimed,
    });

    // Update tournament entry count
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
    event_type: "tournament_joined",
    player_wallet: playerWallet,
    tournament_address: tournamentAddress,
    tx_signature: sig,
    block_time: tx.timestamp,
    raw_data: { entry_deposit: entryData.entryDeposit.toString() },
  });

  WebSocketService.broadcast(WS_EVENTS.TOURNAMENT_JOINED, {
    tournament: tournamentAddress,
    player: playerWallet,
    total_entries: tournamentData.totalEntries,
  });

  console.log(`[onTournamentJoined] Player ${playerWallet.slice(0, 8)}... joined ${tournamentAddress.slice(0, 8)}...`);
}
