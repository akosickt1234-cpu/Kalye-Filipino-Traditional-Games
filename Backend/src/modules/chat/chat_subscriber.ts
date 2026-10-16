import { EVENT_NAMES, event_bus } from "../../core/events/event_bus"
import { add_member, close_lobby, open_lobby, remove_member } from "./chat_store"

let unsubscribers: (() => void)[] = []

export function start_chat_subscriber(): void {
  stop_chat_subscriber()
  unsubscribers = [
    event_bus.on(EVENT_NAMES.LOBBY_OPENED, ({ code }) => open_lobby(code)),
    event_bus.on(EVENT_NAMES.LOBBY_PLAYER_JOINED, ({ code, player_id }) => add_member(code, player_id)),
    event_bus.on(EVENT_NAMES.LOBBY_PLAYER_LEFT, ({ code, player_id }) => remove_member(code, player_id)),
    event_bus.on(EVENT_NAMES.LOBBY_CLOSED, ({ code }) => close_lobby(code))
  ]
}

export function stop_chat_subscriber(): void {
  for (const unsubscribe of unsubscribers) unsubscribe()
  unsubscribers = []
}
