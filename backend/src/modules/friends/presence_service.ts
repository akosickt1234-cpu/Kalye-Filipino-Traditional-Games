import { friends_config } from "../../config/friends_config"
import { EVENT_NAMES, event_bus } from "../../core/events/event_bus"
import type { PresenceStatus } from "./friends_types"

const GUEST_PREFIX = "guest_"

const last_seen = new Map<string, number>()
const lobby_of_player = new Map<string, string>()
const players_of_lobby = new Map<string, Set<string>>()
const in_game_lobbies = new Set<string>()

let unsubscribers: (() => void)[] = []

function is_tracked(player_id: string): boolean {
  return !player_id.startsWith(GUEST_PREFIX)
}

function remove_player(player_id: string, code: string): void {
  if (lobby_of_player.get(player_id) === code) lobby_of_player.delete(player_id)
  const members = players_of_lobby.get(code)
  if (!members) return
  members.delete(player_id)
  if (members.size === 0) {
    players_of_lobby.delete(code)
    in_game_lobbies.delete(code)
  }
}

function add_player(player_id: string, code: string): void {
  const previous = lobby_of_player.get(player_id)
  if (previous && previous !== code) remove_player(player_id, previous)
  lobby_of_player.set(player_id, code)
  const members = players_of_lobby.get(code) ?? new Set<string>()
  members.add(player_id)
  players_of_lobby.set(code, members)
}

export function record_heartbeat(user_id: string): void {
  last_seen.set(user_id, Date.now())
}

export function get_presence(user_id: string): PresenceStatus {
  const seen = last_seen.get(user_id)
  if (seen === undefined || Date.now() - seen > friends_config.presence_timeout_seconds * 1000) {
    return "offline"
  }
  const code = lobby_of_player.get(user_id)
  if (!code) return "online"
  return in_game_lobbies.has(code) ? "in_game" : "in_lobby"
}

export function get_lobby_of(user_id: string): string | null {
  return lobby_of_player.get(user_id) ?? null
}

export function is_lobby_in_game(code: string): boolean {
  return in_game_lobbies.has(code)
}

export function prune_stale_presence(): void {
  const limit = friends_config.presence_timeout_seconds * 1000 * 2
  const now = Date.now()
  for (const [user_id, seen] of last_seen) {
    if (now - seen > limit) last_seen.delete(user_id)
  }
}

export function start_presence_tracking(on_lobby_closed: (code: string) => void, on_player_joined: (code: string, player_id: string) => void): void {
  stop_presence_tracking()
  unsubscribers = [
    event_bus.on(EVENT_NAMES.LOBBY_PLAYER_JOINED, ({ code, player_id }) => {
      if (!is_tracked(player_id)) return
      add_player(player_id, code)
      on_player_joined(code, player_id)
    }),
    event_bus.on(EVENT_NAMES.LOBBY_PLAYER_LEFT, ({ code, player_id }) => {
      if (!is_tracked(player_id)) return
      remove_player(player_id, code)
    }),
    event_bus.on(EVENT_NAMES.GAME_STARTED, ({ lobby_code }) => {
      if (players_of_lobby.has(lobby_code)) in_game_lobbies.add(lobby_code)
    }),
    event_bus.on(EVENT_NAMES.GAME_CANCELLED, ({ lobby_code }) => {
      in_game_lobbies.delete(lobby_code)
    }),
    event_bus.on(EVENT_NAMES.MATCH_FINISHED, ({ lobby_code }) => {
      if (lobby_code) in_game_lobbies.delete(lobby_code)
    }),
    event_bus.on(EVENT_NAMES.LOBBY_CLOSED, ({ code }) => {
      const members = players_of_lobby.get(code)
      if (members) {
        for (const player_id of members) {
          if (lobby_of_player.get(player_id) === code) lobby_of_player.delete(player_id)
        }
      }
      players_of_lobby.delete(code)
      in_game_lobbies.delete(code)
      on_lobby_closed(code)
    })
  ]
}

export function stop_presence_tracking(): void {
  for (const unsubscribe of unsubscribers) unsubscribe()
  unsubscribers = []
}
