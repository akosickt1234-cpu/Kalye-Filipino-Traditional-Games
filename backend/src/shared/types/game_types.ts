export interface GameConfig {
  display_name: string
  min_players: number
  max_players: number
  team_count: number
}

export type MatchOutcome = "win" | "loss" | "draw"

export interface GameModule {
  id: string
  config: GameConfig
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
