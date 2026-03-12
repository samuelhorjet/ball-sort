import { supabase } from "../config/supabase.js";

export interface PlayerRow {
  id: string;
  privy_user_id: string;
  auth_method: "wallet" | "google" | "email";
  wallet_address: string | null;        // external wallet (wallet login only)
  embedded_wallet_address: string | null; // privy-managed embedded wallet
  email: string | null;
  is_email_verified: boolean;
  username: string | null;
  avatar_url: string | null;
  total_solved: number;
  best_score: number;
  current_streak: number;
  longest_streak: number;
  player_auth_created: boolean;         // true once create_player_auth tx confirmed
  player_auth_pubkey: string | null;    // the on-chain PDA address
  last_active_at: string;
  created_at: string;
  updated_at: string;
}

export interface SmartSyncPayload {
  privy_user_id: string;
  auth_method: "wallet" | "google" | "email";
  wallet_address?: string | null;
  embedded_wallet_address?: string | null;
  email?: string | null;
  is_email_verified?: boolean;
  username?: string | undefined;
  avatar_url?: string | undefined;
}

export type SmartSyncStatus = "created" | "updated" | "no_change";

export class PlayerModel {
  /**
   * Upsert a player on login. Mirrors the pattern from the uploaded users_controller.
   * Returns the player row and whether it was created/updated.
   */
  static async smartSync(
    payload: SmartSyncPayload
  ): Promise<{ user: PlayerRow; status: SmartSyncStatus }> {
    // Check if player already exists
    const { data: existing } = await supabase
      .from("players")
      .select("*")
      .eq("privy_user_id", payload.privy_user_id)
      .single();

    if (!existing) {
      // New player — INSERT
      const { data, error } = await supabase
        .from("players")
        .insert({
          privy_user_id: payload.privy_user_id,
          auth_method: payload.auth_method,
          wallet_address: payload.wallet_address ?? null,
          embedded_wallet_address: payload.embedded_wallet_address ?? null,
          email: payload.email ?? null,
          is_email_verified: payload.is_email_verified ?? false,
          username: payload.username ?? null,
          avatar_url: payload.avatar_url ?? null,
          last_active_at: new Date().toISOString(),
        })
        .select()
        .single();

      if (error) throw error;
      return { user: data as PlayerRow, status: "created" };
    }

    // Existing player — UPDATE only changed fields
    const updates: Partial<PlayerRow> = {
      last_active_at: new Date().toISOString(),
      updated_at: new Date().toISOString(),
    };

    if (payload.embedded_wallet_address && !existing.embedded_wallet_address) {
      updates.embedded_wallet_address = payload.embedded_wallet_address;
    }
    if (payload.email && !existing.email) {
      updates.email = payload.email;
    }
    if (payload.username && !existing.username) {
      updates.username = payload.username;
    }

    const { data, error } = await supabase
      .from("players")
      .update(updates)
      .eq("privy_user_id", payload.privy_user_id)
      .select()
      .single();

    if (error) throw error;
    return { user: data as PlayerRow, status: "updated" };
  }

  static async findByPrivyId(privyUserId: string): Promise<PlayerRow | null> {
    const { data, error } = await supabase
      .from("players")
      .select("*")
      .eq("privy_user_id", privyUserId)
      .single();

    if (error?.code === "PGRST116") return null;
    if (error) throw error;
    return data as PlayerRow;
  }

  static async findByWallet(wallet: string): Promise<PlayerRow | null> {
    // Check embedded wallet first, then external wallet
    const { data, error } = await supabase
      .from("players")
      .select("*")
      .or(`embedded_wallet_address.eq.${wallet},wallet_address.eq.${wallet}`)
      .single();

    if (error?.code === "PGRST116") return null;
    if (error) throw error;
    return data as PlayerRow;
  }

  static async updateProfile(
    privyUserId: string,
    updates: { username?: string | undefined; avatar_url?: string | undefined }
  ): Promise<PlayerRow> {
    const { data, error } = await supabase
      .from("players")
      .update({ ...updates, updated_at: new Date().toISOString() })
      .eq("privy_user_id", privyUserId)
      .select()
      .single();

    if (error) throw error;
    return data as PlayerRow;
  }

  static async markPlayerAuthCreated(
    privyUserId: string,
    playerAuthPubkey: string
  ): Promise<void> {
    const { error } = await supabase
      .from("players")
      .update({
        player_auth_created: true,
        player_auth_pubkey: playerAuthPubkey,
        updated_at: new Date().toISOString(),
      })
      .eq("privy_user_id", privyUserId);

    if (error) throw error;
  }

  static async incrementSolved(wallet: string, score: number): Promise<void> {
    // Use Supabase RPC to atomically update stats
    const { error } = await supabase.rpc("increment_player_solved", {
      p_wallet: wallet,
      p_score: score,
    });

    if (error) throw error;
  }

  static async updateStreak(wallet: string, increment: boolean): Promise<void> {
    const { error } = await supabase.rpc(increment ? "increment_streak" : "reset_streak", {
      p_wallet: wallet,
    });

    if (error) throw error;
  }
}
