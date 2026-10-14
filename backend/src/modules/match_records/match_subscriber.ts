import { event_bus, EVENT_NAMES } from "../../core/events/event_bus"
import { child_logger } from "../../core/logging/logger"
import { save_match } from "./match_repository"

const log = child_logger("match_records")

let unsubscribe: (() => void) | null = null

export function start_match_subscriber(): void {
  if (unsubscribe) return
  unsubscribe = event_bus.on(EVENT_NAMES.MATCH_FINISHED, async (match) => {
    const saved = await save_match(match)
    log.info({ match_id: match.match_id, saved }, saved ? "match saved" : "match already saved")
  })
}

export function stop_match_subscriber(): void {
  unsubscribe?.()
  unsubscribe = null
}
