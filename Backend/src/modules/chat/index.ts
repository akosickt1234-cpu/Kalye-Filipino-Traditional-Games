import type { ModuleDefinition } from "../../shared/types/module_types"
import { register_chat_routes } from "./chat_routes"
import { reset_store } from "./chat_store"
import { start_chat_subscriber, stop_chat_subscriber } from "./chat_subscriber"

const definition: ModuleDefinition = {
  name: "chat",
  routes: register_chat_routes,
  async on_start() {
    start_chat_subscriber()
  },
  async on_stop() {
    stop_chat_subscriber()
    reset_store()
  }
}

export default definition
