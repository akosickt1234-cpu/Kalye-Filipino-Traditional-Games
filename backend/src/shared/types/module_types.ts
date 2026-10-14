import type { Room } from "@colyseus/core"
import type { Express, RequestHandler } from "express"

export interface ModuleDefinition {
  name: string
  routes?: (app: Express) => void
  middleware?: { order: number; handler: RequestHandler }[]
  rooms?: { name: string; room_class: new () => Room; options?: object }[]
  jobs?: { name: string; interval_ms: number; run: () => Promise<void> }[]
  room_features?: { room_names: string[]; attach: (room: Room) => void }[]
  on_start?: () => Promise<void>
  on_stop?: () => Promise<void>
}
