import type { FastifyInstance } from "fastify";
import { PuzzleController } from "../controllers/puzzle.controller.js";
import { UserController } from "../controllers/user.controller.js";
import { authenticate } from "../middleware/auth.middleware.js";

export async function puzzleRoutes(app: FastifyInstance): Promise<void> {
  // Public — read puzzle history (delegates to UserController)
  app.get("/puzzles/history/:wallet", UserController.getHistory);

  // Protected — submit finalized puzzle result
  app.post(
    "/puzzles/submit",
    { preHandler: [authenticate] },
    PuzzleController.submitResult
  );
}