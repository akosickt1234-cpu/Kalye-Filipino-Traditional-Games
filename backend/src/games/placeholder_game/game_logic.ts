import { BaseGamePlayer, BaseGameState } from "../../shared/schemas/base_game_state"
import { GAME_STATUS } from "../../shared/constants/game_status"
import type { GameResultEntry } from "../../shared/types/game_types"
import type { PlayerIdentity } from "../../shared/types/identity"
import { game_config } from "./game_config"

export function create_state(players: PlayerIdentity[]): BaseGameState {
  const state = new BaseGameState()
  state.status = GAME_STATUS.PLAYING
  state.time_left = game_config.duration_seconds
  for (const identity of players) {
    const player = new BaseGamePlayer()
    player.id = identity.id
    player.username = identity.username
    state.players.set(identity.id, player)
  }
  return state
}

export function on_player_input(_state: BaseGameState, _player_id: string, _input: unknown): void {}

export function on_player_action(_state: BaseGameState, _player_id: string, _action: unknown): void {}

export function on_tick(state: BaseGameState, delta_ms: number): void {
  state.tick += 1
  state.time_left = Math.max(0, state.time_left - delta_ms / 1000)
}

export function is_finished(state: BaseGameState): boolean {
  return state.time_left <= 0
}

export function get_results(state: BaseGameState): GameResultEntry[] {
  const results: GameResultEntry[] = []
  state.players.forEach((player) => {
    results.push({ player_id: player.id, team: player.team || null, result: "draw", score: player.score })
  })
  return results
}

export function on_player_leave(state: BaseGameState, player_id: string): void {
  const player = state.players.get(player_id)
  if (player) player.connected = false
}
