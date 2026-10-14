import type { Express } from "express"
import { check_database } from "../../infrastructure/database/database_client"

export function register_health_route(app: Express): void {
  app.get("/health", async (_req, res) => {
    const database_up = await check_database()
    res.status(database_up ? 200 : 503).json({
      status: database_up ? "ok" : "degraded",
      uptime: Math.round(process.uptime()),
      db: database_up ? "up" : "down"
    })
  })
}
