import { environment } from "./environment"

export const friends_config = {
  friends_max: environment.FRIENDS_MAX,
  invite_ttl_seconds: environment.INVITE_TTL_SECONDS,
  presence_timeout_seconds: environment.PRESENCE_TIMEOUT_SECONDS
} as const
