import type { EVENT_NAMES } from "../constants/event_names"
import type { MatchResult } from "./game_types"

export type LobbyVisibility = "public" | "private"

export interface EventPayloads {
  [EVENT_NAMES.LOBBY_OPENED]: { code: string; host_id: string; visibility: LobbyVisibility }
  [EVENT_NAMES.LOBBY_CLOSED]: { code: string }
  [EVENT_NAMES.LOBBY_PLAYER_JOINED]: { code: string; player_id: string }
  [EVENT_NAMES.LOBBY_PLAYER_LEFT]: { code: string; player_id: string }
  [EVENT_NAMES.GAME_STARTED]: { lobby_code: string; game_type: string; player_ids: string[] }
  [EVENT_NAMES.GAME_CANCELLED]: { lobby_code: string }
  [EVENT_NAMES.MATCH_FINISHED]: MatchResult
}
