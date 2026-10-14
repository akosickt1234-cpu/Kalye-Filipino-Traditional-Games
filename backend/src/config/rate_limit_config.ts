import { environment } from "./environment"

export const rate_limit_config = {
  window_seconds: environment.RATE_LIMIT_WINDOW_SECONDS,
  max_requests: environment.RATE_LIMIT_MAX,
  auth_max_requests: environment.AUTH_RATE_LIMIT_MAX
} as const
