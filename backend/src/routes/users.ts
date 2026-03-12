import type { FastifyInstance } from "fastify";
import { UserController } from "../controllers/user.controller.js";
import { authenticate } from "../middleware/auth.middleware.js";

export async function userRoutes(app: FastifyInstance): Promise<void> {
  // Public — called on login with raw Privy token in body
  app.post("/users/sync", UserController.syncUser);

  // Public — read player profile by wallet
  app.get("/users/:wallet", UserController.getProfile);

  // Protected — requires Bearer token
  app.patch(
    "/users/profile",
    { preHandler: [authenticate] },
    UserController.updateProfile
  );

  app.patch(
    "/users/player-auth",
    { preHandler: [authenticate] },
    UserController.markPlayerAuth
  );
}