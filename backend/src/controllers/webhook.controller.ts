import type { FastifyRequest, FastifyReply } from "fastify";
import { routeEvent } from "../indexer/eventRouter.js";
import { handleApiError } from "../utils/errorHandler.js";

export class WebhookController {
  /**
   * POST /webhooks/helius
   * Receives an array of enhanced transactions from Helius.
   * Routes each event to the appropriate handler.
   */
  static async handleHelius(req: FastifyRequest, reply: FastifyReply) {
    try {
      const events = req.body as any[];

      if (!Array.isArray(events)) {
        return reply.status(400).send({ error: "Expected array of events." });
      }

      // Process all events concurrently but don't let one failure kill the rest
      const results = await Promise.allSettled(
        events.map((event) => routeEvent(event))
      );

      const failures = results.filter((r) => r.status === "rejected");
      if (failures.length > 0) {
        console.error(
          `[Webhook] ${failures.length}/${events.length} events failed:`,
          failures.map((f) => (f as PromiseRejectedResult).reason?.message)
        );
      }

      // Always return 200 — Helius retries on non-200
      return reply.status(200).send({ received: events.length });
    } catch (error) {
      return handleApiError(reply, error, "WebhookController.handleHeliusEvent");
    }
  }
}
