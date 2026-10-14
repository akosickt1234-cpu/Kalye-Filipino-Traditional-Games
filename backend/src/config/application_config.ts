import { environment } from "./environment"

export const application_config = {
  node_env: environment.NODE_ENV,
  is_production: environment.NODE_ENV === "production",
  port: environment.PORT,
  log_level: environment.LOG_LEVEL,
  long_poll_max_seconds: environment.LONG_POLL_MAX_SECONDS,
  cors_origins: environment.CORS_ORIGINS.split(",")
    .map((origin) => origin.trim())
    .filter((origin) => origin.length > 0)
} as const
