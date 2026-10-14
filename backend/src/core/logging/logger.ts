import pino from "pino"
import { application_config } from "../../config/application_config"

const REDACTED_KEYS = [
  "authorization",
  "password",
  "password_hash",
  "token",
  "access_token",
  "refresh_token",
  "cookie"
]

const redact_paths = [
  ...REDACTED_KEYS,
  ...REDACTED_KEYS.map((key) => `*.${key}`),
  ...REDACTED_KEYS.map((key) => `*.*.${key}`),
  ...REDACTED_KEYS.map((key) => `req.headers.${key}`),
  ...REDACTED_KEYS.map((key) => `headers.${key}`)
]

export const logger = pino({
  level: application_config.log_level,
  redact: { paths: redact_paths, censor: "[redacted]" },
  base: undefined,
  timestamp: pino.stdTimeFunctions.isoTime
})

export function child_logger(name: string) {
  return logger.child({ scope: name })
}
