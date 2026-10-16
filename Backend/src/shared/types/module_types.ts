import type { Express, RequestHandler } from "express"

export interface ModuleDefinition {
  name: string
  routes?: (app: Express) => void
  middleware?: { order: number; handler: RequestHandler }[]
  jobs?: { name: string; interval_ms: number; run: () => Promise<void> }[]
  on_start?: () => Promise<void>
  on_stop?: () => Promise<void>
}
