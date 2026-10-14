export const EVENT_NAMES = {
  LOBBY_OPENED: "lobby_opened",
  LOBBY_CLOSED: "lobby_closed",
  LOBBY_PLAYER_JOINED: "lobby_player_joined",
  LOBBY_PLAYER_LEFT: "lobby_player_left",
  GAME_STARTED: "game_started",
  GAME_CANCELLED: "game_cancelled",
  MATCH_FINISHED: "match_finished"
} as const

export type EventName = (typeof EVENT_NAMES)[keyof typeof EVENT_NAMES]
