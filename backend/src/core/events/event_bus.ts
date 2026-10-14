import { EVENT_NAMES, type EventName } from "../../shared/constants/event_names"
import type { EventPayloads } from "../../shared/types/event_types"
import { child_logger } from "../logging/logger"

type Handler<K extends EventName> = (payload: EventPayloads[K]) => void | Promise<void>

const log = child_logger("event_bus")

class EventBus {
  private handlers = new Map<EventName, Set<Handler<EventName>>>()

  on<K extends EventName>(name: K, handler: Handler<K>): () => void {
    const set = this.handlers.get(name) ?? new Set()
    set.add(handler as Handler<EventName>)
    this.handlers.set(name, set)
    return () => this.off(name, handler)
  }

  once<K extends EventName>(name: K, handler: Handler<K>): () => void {
    const unsubscribe = this.on(name, (payload) => {
      unsubscribe()
      return handler(payload)
    })
    return unsubscribe
  }

  off<K extends EventName>(name: K, handler: Handler<K>): void {
    this.handlers.get(name)?.delete(handler as Handler<EventName>)
  }

  emit<K extends EventName>(name: K, payload: EventPayloads[K]): void {
    const set = this.handlers.get(name)
    if (!set) return
    for (const handler of [...set]) {
      try {
        const result = handler(payload)
        if (result instanceof Promise) {
          result.catch((error) => log.error({ err: error, event: name }, "event handler failed"))
        }
      } catch (error) {
        log.error({ err: error, event: name }, "event handler failed")
      }
    }
  }

  listener_count(name: EventName): number {
    return this.handlers.get(name)?.size ?? 0
  }

  clear(): void {
    this.handlers.clear()
  }
}

export const event_bus = new EventBus()
export { EVENT_NAMES }
