import type { FastifyRequest, FastifyReply } from "fastify";
import { privy, verifyAccessToken } from "../config/privyClient.js";
import { env } from "../config/env.js";
import { PlayerModel } from "../models/Player.js";
import { handleApiError } from "../utils/errorHandler.js";
import { sendSuccess, sendNotFound } from "../utils/response.js";
import {
  syncUserSchema,
  updateProfileSchema,
  walletParamSchema,
  historyQuerySchema,
  playerAuthUpdateSchema,
} from "../utils/validator.js";
import { PuzzleResultModel } from "../models/PuzzleResult.js";
import type { AuthUser } from "../middleware/auth.middleware.js";

export class UserController {
  /**
   * POST /users/sync
   * Called on every Privy login. Creates or updates the player row.
   * Exact pattern from uploaded users_controller.ts
   */
  static async syncUser(req: FastifyRequest, reply: FastifyReply) {
    try {
      const body = syncUserSchema.parse(req.body);

      // 1. Verify token
      const verifiedClaims = await verifyAccessToken({
        access_token: body.token,
        app_id: env.PRIVY_APP_ID,
        verification_key: env.PRIVY_VERIFICATION_KEY,
      });

      // 2. Fetch Privy User
      const privyUser = await privy.users()._get(verifiedClaims.user_id);

      // 3. Separate wallets — exact logic from uploaded reference
      const embeddedWalletObj = privyUser.linked_accounts?.find(
        (acc: any) => acc.type === "wallet" && acc.wallet_client_type === "privy"
      );

      const externalWalletObj = privyUser.linked_accounts?.find(
        (acc: any) => acc.type === "wallet" && acc.wallet_client_type !== "privy"
      );

      const embeddedWalletAddress =
        embeddedWalletObj && "address" in embeddedWalletObj
          ? (embeddedWalletObj as any).address as string
          : null;

      // 4. Determine auth method & email
      const googleAccount = privyUser.linked_accounts?.find(
        (acc: any) => acc.type === "google_oauth"
      );
      const emailAccount = privyUser.linked_accounts?.find(
        (acc: any) => acc.type === "email"
      );

      let authMethod: "wallet" | "google" | "email" = "wallet";
      let email: string | null = null;
      let isEmailVerified = false;
      let finalExternalWalletAddress: string | null = null;

      if (googleAccount && "email" in googleAccount) {
        authMethod = "google";
        email = (googleAccount as any).email;
        isEmailVerified = true;
        // Google users: no external wallet
      } else if (emailAccount && "address" in emailAccount) {
        authMethod = "email";
        email = (emailAccount as any).address;
        isEmailVerified = !!(emailAccount as any).verified_at;
        // Email users: no external wallet
      } else {
        // Wallet login
        authMethod = "wallet";
        if (externalWalletObj && "address" in externalWalletObj) {
          finalExternalWalletAddress = (externalWalletObj as any).address as string;
        } else {
          // Fallback for edge cases with older accounts
          const anyWallet = privyUser.linked_accounts?.find(
            (acc: any) => acc.type === "wallet"
          );
          if (anyWallet && "address" in anyWallet) {
            finalExternalWalletAddress = (anyWallet as any).address as string;
          }
        }
      }

      // 5. Build username from email if none provided
      let username = body.username;
      if (!username && email) {
        username = email.split("@")[0];
      }

      // 6. Sync to DB
      const { user, status } = await PlayerModel.smartSync({
        privy_user_id: privyUser.id,
        auth_method: authMethod,
        wallet_address: finalExternalWalletAddress,
        embedded_wallet_address: embeddedWalletAddress,
        email,
        is_email_verified: isEmailVerified,
        username,
      });

      return sendSuccess(reply, { user, status });
    } catch (error) {
      return handleApiError(reply, error, "UserController.syncUser");
    }
  }

  /**
   * PATCH /users/profile
   * Update username or avatar. Requires Privy token in body.
   */
  static async updateProfile(req: FastifyRequest, reply: FastifyReply) {
    try {
      const body = updateProfileSchema.parse(req.body);

      const verifiedClaims = await verifyAccessToken({
        access_token: body.token,
        app_id: env.PRIVY_APP_ID,
        verification_key: env.PRIVY_VERIFICATION_KEY,
      });

      const user = await PlayerModel.updateProfile(verifiedClaims.user_id, {
        username: body.username,
        avatar_url: body.avatar_url,
      });

      return sendSuccess(reply, { user });
    } catch (error) {
      return handleApiError(reply, error, "UserController.updateProfile");
    }
  }

  /**
   * GET /users/:wallet
   * Public profile by wallet address.
   */
  static async getProfile(req: FastifyRequest, reply: FastifyReply) {
    try {
      const { wallet } = walletParamSchema.parse(req.params);
      const player = await PlayerModel.findByWallet(wallet);

      if (!player) return sendNotFound(reply, "Player");
      return sendSuccess(reply, { player });
    } catch (error) {
      return handleApiError(reply, error, "UserController.getProfile");
    }
  }

  /**
   * GET /users/:wallet/history
   * Puzzle history for a player.
   */
  static async getHistory(req: FastifyRequest, reply: FastifyReply) {
    try {
      const { wallet } = walletParamSchema.parse(req.params);
      const { page, limit } = historyQuerySchema.parse(req.query);

      const { data, total } = await PuzzleResultModel.findByWallet(wallet, page, limit);
      return reply.send({
        success: true,
        data,
        pagination: { page, limit, total, pages: Math.ceil(total / limit) },
      });
    } catch (error) {
      return handleApiError(reply, error, "UserController.getHistory");
    }
  }

  /**
   * PATCH /users/:wallet/player-auth
   * Called by frontend after create_player_auth tx confirms.
   * Marks the player's on-chain auth account as created.
   */
  static async markPlayerAuth(req: FastifyRequest, reply: FastifyReply) {
    try {
      const user = (req as any).user as AuthUser;
      const body = playerAuthUpdateSchema.parse(req.body);

      await PlayerModel.markPlayerAuthCreated(
        user.privyUserId,
        body.player_auth_pubkey
      );

      return sendSuccess(reply, { player_auth_pubkey: body.player_auth_pubkey });
    } catch (error) {
      return handleApiError(reply, error, "UserController.markPlayerAuth");
    }
  }
}
