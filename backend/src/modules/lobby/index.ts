import type { ModuleDefinition } from "../../shared/types/module_types"
import { register_lobby_routes } from "./lobby_routes"
import { release_all_waiters, remove_stale_members } from "./lobby_service"

const JOB_INTERVAL_MS = 5000

const definition: ModuleDefinition = {
  name: "lobby",
  routes: register_lobby_routes,
  jobs: [
    {
      name: "remove_stale_members",
      interval_ms: JOB_INTERVAL_MS,
      run: async () => remove_stale_members()
    }
  ],
  on_stop: async () => release_all_waiters()
}

export default definition
