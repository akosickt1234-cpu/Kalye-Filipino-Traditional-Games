import type { Response } from "express"
import { application_config } from "../../config/application_config"
import { lobby_config } from "../../config/lobby_config"
import { AppError } from "../../core/errors/app_error"
import { event_bus } from "../../core/events/event_bus"
import { game_registry } from "../../core/loaders/game_loader"
import { EVENT_NAMES } from "../../shared/constants/event_names"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import type { PlayerIdentity } from "../../shared/types/identity"
import type { MatchResult } from "../../shared/types/game_types"
import { generate_code } from "../../shared/utils/code_utils"
import type { CreateLobbyInput, ResultInput, UpdateLobbyInput } from "./lobby_schemas"
import type { Lobby, LobbyMember, PublicLobbyView, LobbySummaryView } from "./lobby_types"
import { public_view, summary_view } from "./lobby_views"

const MAX_FINISHED_MATCH_IDS = 50
const MAX_CODE_ATTEMPTS = 100

const lobbies = new Map<string, Lobby>()
const player_lobby = new Map<string, string>()

export function normalize_code(raw: string): string {
  return raw.trim().toUpperCase()
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max)
}

function get_game(game_type: string) {
  const game = game_registry.get(game_type)
  if (!game) throw new AppError(ERROR_CODES.GAME_NOT_FOUND)
  return game
}

export function find_lobby(code: string): Lobby {
  const lobby = lobbies.get(normalize_code(code))
  if (!lobby) throw new AppError(ERROR_CODES.LOBBY_NOT_FOUND)
  return lobby
}

export function require_member(lobby: Lobby, player_id: string): LobbyMember {
  const member = lobby.members.get(player_id)
  if (!member) throw new AppError(ERROR_CODES.FORBIDDEN, "you are not in this lobby")
  return member
}

function require_host(lobby: Lobby, player_id: string): void {
  require_member(lobby, player_id)
  if (lobby.host_id !== player_id) throw new AppError(ERROR_CODES.NOT_HOST)
}

function wake_all(lobby: Lobby): void {
  for (const waiter of [...lobby.waiters]) waiter.wake()
}

function bump(lobby: Lobby): void {
  lobby.version += 1
  wake_all(lobby)
}

function unique_code(): string {
  for (let attempt = 0; attempt < MAX_CODE_ATTEMPTS; attempt += 1) {
    const code = generate_code(lobby_config.code_length)
    if (!lobbies.has(code)) return code
  }
  throw new AppError(ERROR_CODES.INTERNAL_ERROR)
}

function display_name_for(lobby: Lobby, username: string): string {
  const taken = new Set([...lobby.members.values()].map((member) => member.username.toLowerCase()))
  if (!taken.has(username.toLowerCase())) return username
  let suffix = 2
  while (taken.has(`${username} (${suffix})`.toLowerCase())) suffix += 1
  return `${username} (${suffix})`
}

function new_member(player: PlayerIdentity, username: string): LobbyMember {
  const now = Date.now()
  return {
    id: player.id,
    username,
    avatar: player.avatar,
    is_guest: player.is_guest,
    joined_at: now,
    last_heartbeat: now
  }
}

function close_lobby(lobby: Lobby): void {
  lobby.closed = true
  lobbies.delete(lobby.code)
  wake_all(lobby)
  event_bus.emit(EVENT_NAMES.LOBBY_CLOSED, { code: lobby.code })
}

function remove_member(lobby: Lobby, player_id: string): void {
  if (!lobby.members.delete(player_id)) return
  player_lobby.delete(player_id)
  if (lobby.members.size === 0) {
    event_bus.emit(EVENT_NAMES.LOBBY_PLAYER_LEFT, { code: lobby.code, player_id })
    close_lobby(lobby)
    return
  }
  if (lobby.host_id === player_id) {
    lobby.host_id = lobby.members.keys().next().value as string
  }
  bump(lobby)
  event_bus.emit(EVENT_NAMES.LOBBY_PLAYER_LEFT, { code: lobby.code, player_id })
}

function leave_current_lobby(player_id: string): void {
  const code = player_lobby.get(player_id)
  if (!code) return
  const lobby = lobbies.get(code)
  if (!lobby) {
    player_lobby.delete(player_id)
    return
  }
  remove_member(lobby, player_id)
}

export function create_lobby(player: PlayerIdentity, input: CreateLobbyInput): Lobby {
  const game_type = input.game_type ?? lobby_config.default_game_type
  const game = get_game(game_type)
  const max_players = clamp(
    input.max_players ?? game.config.max_players,
    game.config.min_players,
    game.config.max_players
  )
  leave_current_lobby(player.id)
  const now = Date.now()
  const lobby: Lobby = {
    code: unique_code(),
    name: input.name,
    visibility: input.visibility,
    game_type,
    host_id: player.id,
    status: "waiting",
    max_players,
    join_data: input.join_data,
    version: 1,
    created_at: now,
    closed: false,
    members: new Map([[player.id, new_member(player, player.username)]]),
    kicked: new Set(),
    snapshot: new Map(),
    finished_match_ids: new Set(),
    waiters: new Set()
  }
  lobbies.set(lobby.code, lobby)
  player_lobby.set(player.id, lobby.code)
  event_bus.emit(EVENT_NAMES.LOBBY_OPENED, {
    code: lobby.code,
    host_id: player.id,
    visibility: lobby.visibility
  })
  event_bus.emit(EVENT_NAMES.LOBBY_PLAYER_JOINED, { code: lobby.code, player_id: player.id })
  return lobby
}

export function join_lobby(player: PlayerIdentity, code: string): Lobby {
  const lobby = find_lobby(code)
  if (lobby.members.has(player.id)) return lobby
  if (lobby.kicked.has(player.id)) {
    throw new AppError(ERROR_CODES.FORBIDDEN, "you were removed from this lobby")
  }
  if (lobby.status !== "waiting") throw new AppError(ERROR_CODES.LOBBY_IN_GAME)
  if (lobby.members.size >= lobby.max_players) throw new AppError(ERROR_CODES.LOBBY_FULL)
  leave_current_lobby(player.id)
  lobby.members.set(player.id, new_member(player, display_name_for(lobby, player.username)))
  player_lobby.set(player.id, lobby.code)
  bump(lobby)
  event_bus.emit(EVENT_NAMES.LOBBY_PLAYER_JOINED, { code: lobby.code, player_id: player.id })
  return lobby
}

export function leave_lobby(player: PlayerIdentity, code: string): void {
  const lobby = find_lobby(code)
  require_member(lobby, player.id)
  remove_member(lobby, player.id)
}

export function list_public(): PublicLobbyView[] {
  return [...lobbies.values()]
    .filter((lobby) => lobby.visibility === "public" && lobby.status === "waiting")
    .sort((a, b) => b.created_at - a.created_at)
    .map(public_view)
}

export function lobby_summary(code: string): LobbySummaryView {
  return summary_view(find_lobby(code))
}

export function update_lobby(player: PlayerIdentity, code: string, input: UpdateLobbyInput): Lobby {
  const lobby = find_lobby(code)
  require_host(lobby, player.id)

  const settings_changed =
    input.name !== undefined ||
    input.game_type !== undefined ||
    input.max_players !== undefined ||
    input.join_data !== undefined
  if (settings_changed && lobby.status !== "waiting") {
    throw new AppError(ERROR_CODES.LOBBY_IN_GAME)
  }

  let changed = false

  if (settings_changed) {
    const game_type = input.game_type ?? lobby.game_type
    const game = get_game(game_type)
    const max_players = clamp(
      input.max_players ?? lobby.max_players,
      game.config.min_players,
      game.config.max_players
    )
    if (max_players < lobby.members.size) {
      throw new AppError(ERROR_CODES.CONFLICT, "more players are in the lobby than these settings allow")
    }
    lobby.game_type = game_type
    lobby.max_players = max_players
    if (input.name !== undefined) lobby.name = input.name
    if (input.join_data !== undefined) lobby.join_data = input.join_data
    changed = true
  }

  let started = false
  if (input.status === "in_game" && lobby.status === "waiting") {
    const game = get_game(lobby.game_type)
    if (lobby.members.size < game.config.min_players) {
      throw new AppError(ERROR_CODES.NOT_ENOUGH_PLAYERS)
    }
    lobby.status = "in_game"
    lobby.snapshot = new Map(
      [...lobby.members.values()].map((member) => [
        member.id,
        { id: member.id, username: member.username, is_guest: member.is_guest }
      ])
    )
    started = true
    changed = true
  } else if (input.status === "waiting" && lobby.status === "in_game") {
    lobby.status = "waiting"
    lobby.snapshot = new Map()
    changed = true
  }

  if (changed) bump(lobby)
  if (started) {
    event_bus.emit(EVENT_NAMES.GAME_STARTED, {
      lobby_code: lobby.code,
      game_type: lobby.game_type,
      player_ids: [...lobby.snapshot.keys()]
    })
  }
  return lobby
}

export function kick_member(player: PlayerIdentity, code: string, target_id: string): Lobby {
  const lobby = find_lobby(code)
  require_host(lobby, player.id)
  if (target_id === player.id) {
    throw new AppError(ERROR_CODES.VALIDATION_FAILED, "the host cannot kick themselves")
  }
  if (!lobby.members.has(target_id)) {
    throw new AppError(ERROR_CODES.NOT_FOUND, "player is not in this lobby")
  }
  lobby.kicked.add(target_id)
  remove_member(lobby, target_id)
  return lobby
}

export function submit_result(player: PlayerIdentity, code: string, input: ResultInput): Lobby {
  const lobby = find_lobby(code)
  require_host(lobby, player.id)

  const match_id = input.match_id.toLowerCase()
  if (lobby.finished_match_ids.has(match_id)) return lobby

  if (lobby.status !== "in_game") {
    throw new AppError(ERROR_CODES.CONFLICT, "the lobby is not in a game")
  }

  const started_at = new Date(input.started_at)
  const ended_at = new Date(input.ended_at)
  if (ended_at.getTime() < started_at.getTime()) {
    throw new AppError(ERROR_CODES.VALIDATION_FAILED, undefined, {
      fields: [{ path: "ended_at", message: "ended_at must not be before started_at" }]
    })
  }

  const seen = new Set<string>()
  const players: MatchResult["players"] = input.players.map((entry, index) => {
    const known = lobby.snapshot.get(entry.player_id)
    if (!known || seen.has(entry.player_id)) {
      throw new AppError(ERROR_CODES.VALIDATION_FAILED, undefined, {
        fields: [{ path: `players.${index}.player_id`, message: "player is not part of this match" }]
      })
    }
    seen.add(entry.player_id)
    return {
      player_id: known.id,
      user_id: known.is_guest ? null : known.id,
      is_guest: known.is_guest,
      username: known.username,
      team: entry.team ?? null,
      result: entry.result,
      score: entry.score
    }
  })

  lobby.finished_match_ids.add(match_id)
  if (lobby.finished_match_ids.size > MAX_FINISHED_MATCH_IDS) {
    const oldest = lobby.finished_match_ids.values().next().value as string
    lobby.finished_match_ids.delete(oldest)
  }
  lobby.status = "waiting"
  lobby.snapshot = new Map()
  bump(lobby)

  event_bus.emit(EVENT_NAMES.MATCH_FINISHED, {
    match_id,
    game_type: lobby.game_type,
    lobby_code: lobby.code,
    started_at: started_at.toISOString(),
    ended_at: ended_at.toISOString(),
    players
  })
  return lobby
}

export type HoldOutcome = "woken" | "timeout" | "disconnected"

export function touch_member(lobby: Lobby, player_id: string): void {
  const member = lobby.members.get(player_id)
  if (member) member.last_heartbeat = Date.now()
}

export function hold_for_change(
  lobby: Lobby,
  player_id: string,
  wait_seconds: number,
  res: Response
): Promise<HoldOutcome> {
  const capped = Math.min(wait_seconds, application_config.long_poll_max_seconds)
  return new Promise((resolve) => {
    let done = false
    const waiter = { player_id, wake: () => finish("woken") }
    const timer = setTimeout(() => finish("timeout"), capped * 1000)
    const on_close = () => finish("disconnected")

    function finish(outcome: HoldOutcome): void {
      if (done) return
      done = true
      clearTimeout(timer)
      lobby.waiters.delete(waiter)
      res.off("close", on_close)
      touch_member(lobby, player_id)
      resolve(outcome)
    }

    res.on("close", on_close)
    lobby.waiters.add(waiter)
  })
}

export function remove_stale_members(): void {
  const cutoff = Date.now() - lobby_config.heartbeat_timeout_seconds * 1000
  for (const lobby of [...lobbies.values()]) {
    const held = new Set([...lobby.waiters].map((waiter) => waiter.player_id))
    for (const member of [...lobby.members.values()]) {
      if (lobby.closed) break
      if (held.has(member.id)) continue
      if (member.last_heartbeat < cutoff) remove_member(lobby, member.id)
    }
    if (!lobby.closed && lobby.members.size === 0) close_lobby(lobby)
  }
}

export function release_all_waiters(): void {
  for (const lobby of lobbies.values()) wake_all(lobby)
}
