import { environment } from "./environment"

export const application_config = {
  node_env: environment.NODE_ENV,
  is_production: environment.NODE_ENV === "production",
  port: environment.PORT,
  log_level: environment.LOG_LEVEL,
  ws_max_payload_bytes: environment.WS_MAX_PAYLOAD_BYTES,
  cors_origins: environment.CORS_ORIGINS.split(",")
    .map((origin) => origin.trim())
    .filter((origin) => origin.length > 0),
  monitor_user: environment.MONITOR_USER,
  monitor_password: environment.MONITOR_PASSWORD
} as const
