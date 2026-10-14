import cors from "cors"
import { application_config } from "../../config/application_config"

const ALLOWED_ORIGINS = new Set(application_config.cors_origins)
const PREFLIGHT_MAX_AGE_SECONDS = 600

export const cors_policy = cors({
  origin(origin, callback) {
    callback(null, !origin || ALLOWED_ORIGINS.has(origin))
  },
  methods: ["GET", "POST", "PATCH", "DELETE", "OPTIONS"],
  allowedHeaders: ["Authorization", "Content-Type"],
  exposedHeaders: ["X-Request-Id", "Retry-After"],
  maxAge: PREFLIGHT_MAX_AGE_SECONDS
})
