import type { GameModule } from "../../shared/types/game_types"
import { game_config } from "./game_config"
import {
  create_state,
  get_results,
  is_finished,
  on_player_action,
  on_player_input,
  on_player_leave,
  on_tick
} from "./game_logic"

const placeholder_game: GameModule = {
  id: "placeholder_game",
  config: game_config,
  create_state,
  on_player_input,
  on_player_action,
  on_tick,
  is_finished,
  get_results,
  on_player_leave
}

export default placeholder_game
