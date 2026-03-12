import { supabase } from "../config/supabase.js";

export type TxStatus = "pending" | "success" | "failed";

export type TxType =
  | "create_player_auth"
  | "initialize_puzzle"
  | "start_puzzle"
  | "apply_move"
  | "apply_undo"
  | "abandon_puzzle"
  | "finalize_puzzle"
  | "join_tournament"
  | "record_tournament_result"
  | "close_tournament"
  | "claim_prize"
  | "open_session"
  | "close_session"
  | "update_game_config"
  | "unknown";

export interface TransactionRow {
  id: string;
  signature: string;
  player_wallet: string | null;
  tx_type: TxType;
  status: TxStatus;
  slot: number | null;
  block_time: string | null;          // ISO timestamp
  fee_lamports: number | null;
  compute_units_consumed: number | null;
  error_message: string | null;
  raw_meta: Record<string, unknown> | null; // Helius enhanced tx meta
  created_at: string;
  updated_at: string;
}

export interface RecordTxPayload {
  signature: string;
  player_wallet?: string;
  tx_type: TxType;
  status: TxStatus;
  slot?: number;
  block_time?: number;               // unix seconds
  fee_lamports?: number;
  compute_units_consumed?: number;
  error_message?: string;
  raw_meta?: Record<string, unknown>;
}

export class TransactionModel {
  static async record(payload: RecordTxPayload): Promise<TransactionRow> {
    const { data, error } = await supabase
      .from("transactions")
      .upsert(
        {
          signature: payload.signature,
          player_wallet: payload.player_wallet ?? null,
          tx_type: payload.tx_type,
          status: payload.status,
          slot: payload.slot ?? null,
          block_time: payload.block_time
            ? new Date(payload.block_time * 1000).toISOString()
            : null,
          fee_lamports: payload.fee_lamports ?? null,
          compute_units_consumed: payload.compute_units_consumed ?? null,
          error_message: payload.error_message ?? null,
          raw_meta: payload.raw_meta ?? null,
          updated_at: new Date().toISOString(),
        },
        { onConflict: "signature" }
      )
      .select()
      .single();

    if (error) throw error;
    return data as TransactionRow;
  }

  static async findBySignature(signature: string): Promise<TransactionRow | null> {
    const { data, error } = await supabase
      .from("transactions")
      .select("*")
      .eq("signature", signature)
      .single();

    if (error?.code === "PGRST116") return null;
    if (error) throw error;
    return data as TransactionRow;
  }

  static async findByPlayer(
    playerWallet: string,
    page = 1,
    limit = 20
  ): Promise<{ data: TransactionRow[]; total: number }> {
    const from = (page - 1) * limit;
    const to = from + limit - 1;

    const { data, error, count } = await supabase
      .from("transactions")
      .select("*", { count: "exact" })
      .eq("player_wallet", playerWallet)
      .order("created_at", { ascending: false })
      .range(from, to);

    if (error) throw error;
    return { data: (data ?? []) as TransactionRow[], total: count ?? 0 };
  }

  /**
   * Infer tx type from Helius instruction names in the event.
   */
  static inferTxType(instructionNames: string[]): TxType {
    const name = instructionNames[0]?.toLowerCase() ?? "";
    const map: Record<string, TxType> = {
      create_player_auth: "create_player_auth",
      initialize_puzzle: "initialize_puzzle",
      start_puzzle: "start_puzzle",
      apply_move: "apply_move",
      apply_undo: "apply_undo",
      abandon_puzzle: "abandon_puzzle",
      finalize_puzzle: "finalize_puzzle",
      join_tournament: "join_tournament",
      record_tournament_result: "record_tournament_result",
      close_tournament: "close_tournament",
      claim_prize: "claim_prize",
      open_session: "open_session",
      close_session: "close_session",
      update_game_config: "update_game_config",
    };
    return map[name] ?? "unknown";
  }
}
