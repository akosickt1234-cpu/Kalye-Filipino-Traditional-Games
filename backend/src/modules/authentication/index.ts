import type { ModuleDefinition } from "../../shared/types/module_types"
import { authentication_rate_limit } from "./authentication_rate_limit"
import { register_authentication_routes } from "./authentication_routes"
import { delete_expired_refresh_tokens } from "./refresh_token_store"

const HOUR_MS = 60 * 60 * 1000

const authentication_module: ModuleDefinition = {
  name: "authentication",
  middleware: [{ order: 40, handler: authentication_rate_limit }],
  routes: register_authentication_routes,
  jobs: [
    {
      name: "delete_expired_refresh_tokens",
      interval_ms: HOUR_MS,
      run: async () => {
        await delete_expired_refresh_tokens()
      }
    }
  ]
}

export default authentication_module
