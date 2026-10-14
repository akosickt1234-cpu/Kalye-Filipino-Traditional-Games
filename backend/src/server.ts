import { createServer } from "node:http"
import { Server } from "@colyseus/core"
import { WebSocketTransport } from "@colyseus/ws-transport"
import { create_app } from "./app"
import { application_config } from "./config/application_config"
import { start_jobs, stop_jobs } from "./core/lifecycle/job_runner"
import { game_registry } from "./core/loaders/game_loader"
import { module_registry } from "./core/loaders/module_loader"
import { child_logger } from "./core/logging/logger"
import { check_database, close_database } from "./infrastructure/database/database_client"

const log = child_logger("server")

async function main(): Promise<void> {
  const modules = module_registry.load()
  game_registry.load()

  const app = create_app(modules)
  const http_server = createServer(app)
  const game_server = new Server({
    transport: new WebSocketTransport({
      server: http_server,
      maxPayload: application_config.ws_max_payload_bytes
    }),
    gracefullyShutdown: false,
    greet: false
  })

  for (const definition of modules) {
    for (const room of definition.rooms ?? []) {
      game_server.define(room.name, room.room_class, room.options)
      log.info({ module: definition.name, room: room.name }, "room defined")
    }
  }

  if (!(await check_database())) {
    log.warn("database is not reachable, /health will report db down")
  }

  for (const definition of modules) {
    await definition.on_start?.()
  }

  await game_server.listen(application_config.port)
  start_jobs(modules)
  log.info({ port: application_config.port, env: application_config.node_env }, "server started")

  let shutting_down = false
  async function shutdown(signal: string): Promise<void> {
    if (shutting_down) return
    shutting_down = true
    log.info({ signal }, "shutting down")
    const force_exit = setTimeout(() => process.exit(1), 15000)
    force_exit.unref()
    try {
      await stop_jobs()
      await game_server.gracefullyShutdown(false)
      for (const definition of [...modules].reverse()) {
        try {
          await definition.on_stop?.()
        } catch (error) {
          log.error({ err: error, module: definition.name }, "module stop failed")
        }
      }
      await close_database()
      log.info("shutdown complete")
      process.exit(0)
    } catch (error) {
      log.error({ err: error }, "shutdown failed")
      process.exit(1)
    }
  }

  process.on("SIGINT", () => void shutdown("SIGINT"))
  process.on("SIGTERM", () => void shutdown("SIGTERM"))
}

main().catch((error) => {
  log.fatal({ err: error }, "server failed to start")
  process.exit(1)
})
