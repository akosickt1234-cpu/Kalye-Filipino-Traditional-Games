import type { LobbyVisibility } from "../../shared/types/event_types"

export interface LobbyMember {
  id: string
  username: string
  avatar: string
  is_guest: boolean
  joined_at: number
  last_heartbeat: number
}

export interface SnapshotPlayer {
  id: string
  username: string
  is_guest: boolean
}

export interface Waiter {
  player_id: string
  wake: () => void
}

export interface Lobby {
  code: string
  name: string
  visibility: LobbyVisibility
  game_type: string
  host_id: string
  status: "waiting" | "in_game"
  max_players: number
  join_data: string
  version: number
  created_at: number
  closed: boolean
  members: Map<string, LobbyMember>
  kicked: Set<string>
  snapshot: Map<string, SnapshotPlayer>
  finished_match_ids: Set<string>
  waiters: Set<Waiter>
}

export interface LobbyPlayerView {
  id: string
  username: string
  avatar: string
  is_guest: boolean
  is_host: boolean
}

export interface LobbyView {
  code: string
  version: number
  name: string
  visibility: LobbyVisibility
  game_type: string
  host_id: string
  status: "waiting" | "in_game"
  max_players: number
  players: LobbyPlayerView[]
}

export interface LobbyMemberView extends LobbyView {
  join_data: string
}

export interface PublicLobbyView {
  code: string
  name: string
  host_username: string
  game_type: string
  player_count: number
  max_players: number
}

export interface LobbySummaryView {
  code: string
  name: string
  status: "waiting" | "in_game"
  player_count: number
  max_players: number
}
