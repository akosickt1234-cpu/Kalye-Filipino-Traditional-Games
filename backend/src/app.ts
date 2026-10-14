import express, { type Express } from "express"
import { error_handler, not_found_handler } from "./core/middleware/error_middleware"
import { request_logger } from "./core/middleware/request_logger"
import { register_health_route } from "./core/lifecycle/health"
import type { ModuleDefinition } from "./shared/types/module_types"
import "./shared/types/express"

const FOUNDATION_JSON_LIMIT = "10kb"

export function create_app(modules: ModuleDefinition[]): Express {
  const app = express()
  app.disable("x-powered-by")

  const middleware = [
    { order: 10, handler: request_logger },
    { order: 15, handler: express.json({ limit: FOUNDATION_JSON_LIMIT }) },
    ...modules.flatMap((definition) => definition.middleware ?? [])
  ].sort((a, b) => a.order - b.order)

  for (const entry of middleware) {
    app.use(entry.handler)
  }

  register_health_route(app)

  for (const definition of modules) {
    definition.routes?.(app)
  }

  app.use(not_found_handler)
  app.use(error_handler)
  return app
}
