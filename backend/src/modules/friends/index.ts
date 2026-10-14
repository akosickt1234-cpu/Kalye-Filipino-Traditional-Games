import type { ModuleDefinition } from "../../shared/types/module_types"
import { child_logger } from "../../core/logging/logger"
import { create_friends_router, create_invites_router, create_presence_router } from "./friends_routes"
import {
  delete_expired_invites,
  drop_invites_for_closed_lobby,
  mark_invites_accepted
} from "./invite_service"
import { prune_stale_presence, start_presence_tracking, stop_presence_tracking } from "./presence_service"

const log = child_logger("friends")

const definition: ModuleDefinition = {
  name: "friends",
  routes(app) {
    app.use("/friends", create_friends_router())
    app.use("/invites", create_invites_router())
    app.use("/presence", create_presence_router())
  },
  jobs: [
    {
      name: "friends_delete_expired_invites",
      interval_ms: 60_000,
      run: async () => {
        await delete_expired_invites()
        prune_stale_presence()
      }
    }
  ],
  async on_start() {
    start_presence_tracking(
      (code) => {
        drop_invites_for_closed_lobby(code).catch((error) => log.error({ err: error }, "drop invites failed"))
      },
      (code, player_id) => {
        mark_invites_accepted(code, player_id).catch((error) => log.error({ err: error }, "mark invites failed"))
      }
    )
  },
  async on_stop() {
    stop_presence_tracking()
  }
}

export default definition
