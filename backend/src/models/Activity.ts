import { supabase } from "../config/supabase.js";

export type ActivityEventType =
  | "puzzle_initialized"
  | "puzzle_started"
  | "puzzle_finalized"
  | "puzzle_abandoned"
  | "tournament_created"
  | "tournament_joined"
  | "tournament_closed"
  | "tournament_result_recorded"
  | "prize_claimed"
  | "game_config_initialized"
  | "game_config_updated"
  | "permission_created";

export interface ActivityRow {
  id: string;
  event_type: ActivityEventType;
  player_wallet: string | null;
  tournament_address: string | null;
  puzzle_board_pubkey: string | null;
  puzzle_stats_pubkey: string | null;
  tx_signature: string | null;
  slot: number | null;
  block_time: string | null; // ISO timestamp
  raw_data: Record<string, unknown>; // Full event payload stored as JSONB
  created_at: string;
}

export interface LogActivityPayload {
  event_type: ActivityEventType;
  player_wallet?: string;
  tournament_address?: string;
  puzzle_board_pubkey?: string;
  puzzle_stats_pubkey?: string;
  tx_signature?: string;
  slot?: number;
  block_time?: number; // unix seconds
  raw_data: Record<string, unknown>;
}

export class ActivityModel {
  static async log(payload: LogActivityPayload): Promise<ActivityRow> {
    const { data, error } = await supabase
      .from("activity")
      .insert({
        event_type: payload.event_type,
        player_wallet: payload.player_wallet ?? null,
        tournament_address: payload.tournament_address ?? null,
        puzzle_board_pubkey: payload.puzzle_board_pubkey ?? null,
        puzzle_stats_pubkey: payload.puzzle_stats_pubkey ?? null,
        tx_signature: payload.tx_signature ?? null,
        slot: payload.slot ?? null,
        block_time: payload.block_time
          ? new Date(payload.block_time * 1000).toISOString()
          : null,
        raw_data: payload.raw_data,
      })
      .select()
      .single();

    if (error) throw error;
    return data as ActivityRow;
  }

  static async findByPlayer(
    playerWallet: string,
    limit = 50,
  ): Promise<ActivityRow[]> {
    const { data, error } = await supabase
      .from("activity")
      .select("*")
      .eq("player_wallet", playerWallet)
      .order("created_at", { ascending: false })
      .limit(limit);

    if (error) throw error;
    return (data ?? []) as ActivityRow[];
  }

  static async findByTournament(
    tournamentAddress: string,
    limit = 100,
  ): Promise<ActivityRow[]> {
    const { data, error } = await supabase
      .from("activity")
      .select("*")
      .eq("tournament_address", tournamentAddress)
      .order("created_at", { ascending: false })
      .limit(limit);

    if (error) throw error;
    return (data ?? []) as ActivityRow[];
  }

  static async findRecent(limit = 100): Promise<ActivityRow[]> {
    const { data, error } = await supabase
      .from("activity")
      .select("*")
      .order("created_at", { ascending: false })
      .limit(limit);

    if (error) throw error;
    return (data ?? []) as ActivityRow[];
  }

  /**
   * Check if we already processed a tx to avoid duplicate event handling.
   */
  static async alreadyProcessed(txSignature: string): Promise<boolean> {
    const { data } = await supabase
      .from("activity")
      .select("id")
      .eq("tx_signature", txSignature)
      .limit(1)
      .single();

    return !!data;
  }
}
