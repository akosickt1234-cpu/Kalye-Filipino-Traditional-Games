import type { BaseGameState } from "../schemas/base_game_state"
import type { PlayerIdentity } from "./identity"

export interface GameConfig {
  display_name: string
  min_players: number
  max_players: number
  team_count: number
  duration_seconds: number
  tick_rate: number
  patch_rate: number
  join_timeout_seconds: number
  reconnect_seconds: number
}

export type MatchOutcome = "win" | "loss" | "draw"

export interface GameResultEntry {
  player_id: string
  team: string | null
  result: MatchOutcome
  score: number
}

export interface GameModule {
  id: string
  config: GameConfig
  create_state(players: PlayerIdentity[]): BaseGameState
  on_player_input(state: BaseGameState, player_id: string, input: unknown): void
  on_player_action(state: BaseGameState, player_id: string, action: unknown): void
  on_tick(state: BaseGameState, delta_ms: number): void
  is_finished(state: BaseGameState): boolean
  get_results(state: BaseGameState): GameResultEntry[]
  on_player_leave?(state: BaseGameState, player_id: string): void
}

export interface MatchResult {
  match_id: string
  game_type: string
  lobby_code: string | null
  started_at: string
  ended_at: string
  players: {
    player_id: string
    user_id: string | null
    is_guest: boolean
    username: string
    team: string | null
    result: MatchOutcome
    score: number
  }[]
}
