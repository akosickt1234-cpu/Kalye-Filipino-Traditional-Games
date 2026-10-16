import type { Response } from "express"
import { chat_config } from "../../config/chat_config"
import { AppError } from "../../core/errors/app_error"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { create_rate_limiter } from "../../shared/utils/rate_limiter"

export interface ChatMessage {
  id: number
  from_id: string
  username: string
  text: string
  sent_at: string
}

interface Waiter {
  player_id: string
  after: number
  finish: (outcome: Outcome) => void
}

type Outcome = { messages: ChatMessage[] } | { error: AppError }

interface LobbyChat {
  members: Set<string>
  messages: ChatMessage[]
  next_id: number
  waiters: Set<Waiter>
}

const lobbies = new Map<string, LobbyChat>()
const send_limiter = create_rate_limiter(chat_config.rate_messages, chat_config.rate_window_seconds * 1000)

function key_for(code: string): string {
  return code.toUpperCase()
}

function get_or_create(code: string): LobbyChat {
  const key = key_for(code)
  const existing = lobbies.get(key)
  if (existing) return existing
  const created: LobbyChat = { members: new Set(), messages: [], next_id: 1, waiters: new Set() }
  lobbies.set(key, created)
  return created
}

function require_member(code: string, player_id: string): LobbyChat {
  const lobby = lobbies.get(key_for(code))
  if (!lobby) throw new AppError(ERROR_CODES.LOBBY_NOT_FOUND)
  if (!lobby.members.has(player_id)) throw new AppError(ERROR_CODES.FORBIDDEN)
  return lobby
}

function newer_than(lobby: LobbyChat, after: number): ChatMessage[] {
  return lobby.messages.filter((message) => message.id > after)
}

export function open_lobby(code: string): void {
  get_or_create(code)
}

export function add_member(code: string, player_id: string): void {
  get_or_create(code).members.add(player_id)
}

export function remove_member(code: string, player_id: string): void {
  const lobby = lobbies.get(key_for(code))
  if (!lobby) return
  lobby.members.delete(player_id)
  send_limiter.reset(player_id)
  for (const waiter of [...lobby.waiters]) {
    if (waiter.player_id === player_id) waiter.finish({ error: new AppError(ERROR_CODES.FORBIDDEN) })
  }
}

export function close_lobby(code: string): void {
  const key = key_for(code)
  const lobby = lobbies.get(key)
  if (!lobby) return
  lobbies.delete(key)
  for (const waiter of [...lobby.waiters]) {
    waiter.finish({ error: new AppError(ERROR_CODES.LOBBY_NOT_FOUND) })
  }
}

export function reset_store(): void {
  for (const lobby of lobbies.values()) {
    for (const waiter of [...lobby.waiters]) waiter.finish({ messages: [] })
  }
  lobbies.clear()
  send_limiter.clear()
}

export function check_membership(code: string, player_id: string): void {
  require_member(code, player_id)
}

export function allow_send(player_id: string): void {
  if (!send_limiter.allow(player_id)) throw new AppError(ERROR_CODES.CHAT_RATE_LIMITED)
}

export function add_message(code: string, player_id: string, username: string, text: string): ChatMessage {
  const lobby = require_member(code, player_id)
  const message: ChatMessage = {
    id: lobby.next_id,
    from_id: player_id,
    username,
    text,
    sent_at: new Date().toISOString()
  }
  lobby.next_id += 1
  lobby.messages.push(message)
  if (lobby.messages.length > chat_config.history_size) {
    lobby.messages.splice(0, lobby.messages.length - chat_config.history_size)
  }
  for (const waiter of [...lobby.waiters]) {
    waiter.finish({ messages: newer_than(lobby, waiter.after) })
  }
  return message
}

export async function read_messages(
  code: string,
  player_id: string,
  after: number | undefined,
  wait_seconds: number,
  res: Response
): Promise<ChatMessage[] | null> {
  const lobby = require_member(code, player_id)
  if (after === undefined) return [...lobby.messages]
  const ready = newer_than(lobby, after)
  if (ready.length > 0 || wait_seconds <= 0) return ready

  return new Promise<ChatMessage[] | null>((resolve, reject) => {
    let timer: NodeJS.Timeout | undefined
    const waiter: Waiter = {
      player_id,
      after,
      finish(outcome) {
        cleanup()
        if ("error" in outcome) reject(outcome.error)
        else resolve(outcome.messages)
      }
    }
    const on_close = () => {
      if (res.writableEnded) return
      cleanup()
      resolve(null)
    }
    function cleanup() {
      if (timer) clearTimeout(timer)
      lobby.waiters.delete(waiter)
      res.off("close", on_close)
    }
    lobby.waiters.add(waiter)
    res.on("close", on_close)
    timer = setTimeout(() => waiter.finish({ messages: newer_than(lobby, after) }), wait_seconds * 1000)
  })
}

export function held_request_count(): number {
  let total = 0
  for (const lobby of lobbies.values()) total += lobby.waiters.size
  return total
}
