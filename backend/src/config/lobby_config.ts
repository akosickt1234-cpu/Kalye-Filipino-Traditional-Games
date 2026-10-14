import { environment } from "./environment"

export const lobby_config = {
  code_length: environment.LOBBY_CODE_LENGTH,
  countdown_seconds: environment.LOBBY_COUNTDOWN_SECONDS,
  empty_dispose_seconds: environment.LOBBY_EMPTY_DISPOSE_SECONDS,
  reconnect_seconds: environment.LOBBY_RECONNECT_SECONDS,
  default_game_type: environment.DEFAULT_GAME_TYPE,
  friends_max: environment.FRIENDS_MAX,
  invite_ttl_seconds: environment.INVITE_TTL_SECONDS
} as const
