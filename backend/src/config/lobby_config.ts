import { environment } from "./environment"

export const lobby_config = {
  code_length: environment.LOBBY_CODE_LENGTH,
  heartbeat_timeout_seconds: environment.LOBBY_HEARTBEAT_TIMEOUT_SECONDS,
  default_game_type: environment.DEFAULT_GAME_TYPE
} as const
