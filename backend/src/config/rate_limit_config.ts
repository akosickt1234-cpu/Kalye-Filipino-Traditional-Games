import { environment } from "./environment"

export const rate_limit_config = {
  window_seconds: environment.RATE_LIMIT_WINDOW_SECONDS,
  max_requests: environment.RATE_LIMIT_MAX,
  auth_max_requests: environment.AUTH_RATE_LIMIT_MAX,
  game_input_per_second: environment.GAME_INPUT_RATE_PER_SECOND
} as const
