import type { ModuleDefinition } from "../../shared/types/module_types"
import { register_match_routes } from "./match_routes"
import { start_match_subscriber, stop_match_subscriber } from "./match_subscriber"

const definition: ModuleDefinition = {
  name: "match_records",
  routes: register_match_routes,
  async on_start() {
    start_match_subscriber()
  },
  async on_stop() {
    stop_match_subscriber()
  }
}

export default definition
