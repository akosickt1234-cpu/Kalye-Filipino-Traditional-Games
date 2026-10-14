import type { GameConfig } from "../../shared/types/game_types"

export const game_config: GameConfig = {
  display_name: "Placeholder Game",
  min_players: 2,
  max_players: 6,
  team_count: 1,
  duration_seconds: 60,
  tick_rate: 20,
  patch_rate: 20,
  join_timeout_seconds: 15,
  reconnect_seconds: 20
}
