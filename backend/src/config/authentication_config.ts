import { environment } from "./environment"

export const authentication_config = {
  jwt_access_secret: environment.JWT_ACCESS_SECRET,
  jwt_access_ttl_seconds: environment.JWT_ACCESS_TTL_SECONDS,
  jwt_guest_ttl_seconds: environment.JWT_GUEST_TTL_SECONDS,
  refresh_token_ttl_days: environment.REFRESH_TOKEN_TTL_DAYS
} as const
