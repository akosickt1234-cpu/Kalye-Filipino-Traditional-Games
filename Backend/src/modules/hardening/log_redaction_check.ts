import pino from "pino"
import { logger } from "../../core/logging/logger"

const SECRET_VALUE = "redaction_probe_value"
const PROBE_KEYS = ["authorization", "password", "token", "refresh_token"]

function build_probe(): Record<string, unknown> {
  const flat = Object.fromEntries(PROBE_KEYS.map((key) => [key, SECRET_VALUE]))
  return {
    ...flat,
    body: { ...flat },
    req: { headers: { authorization: SECRET_VALUE } }
  }
}

export function assert_log_redaction(): void {
  const stream_key = pino.symbols.streamSym
  const target = logger as unknown as Record<symbol, unknown>
  const original = target[stream_key]
  const original_level = logger.level
  let captured = ""
  target[stream_key] = {
    write(line: string) {
      captured += line
    }
  }
  try {
    logger.level = "fatal"
    logger.fatal(build_probe(), "log redaction probe")
  } finally {
    target[stream_key] = original
    logger.level = original_level
  }
  if (captured.length === 0 || captured.includes(SECRET_VALUE)) {
    throw new Error("logger redaction check failed, sensitive fields can reach the logs")
  }
}
