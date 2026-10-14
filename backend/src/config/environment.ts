import { existsSync } from "node:fs"
import { z } from "zod"

if (existsSync(".env")) {
  process.loadEnvFile(".env")
}

const DEFAULT_JWT_SECRET = "change_me_change_me_change_me"
const DEFAULT_MONITOR_PASSWORD = "change_me"

const positive_int = z.coerce.number().int().positive()

const environment_schema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]).default("development"),
  PORT: positive_int.max(65535).default(2567),
  LOG_LEVEL: z.enum(["fatal", "error", "warn", "info", "debug", "trace", "silent"]).default("info"),
  DATABASE_URL: z.string().min(1).default("postgres://kftg:kftg@localhost:5432/kftg"),
  WS_MAX_PAYLOAD_BYTES: positive_int.default(4096),
  JWT_ACCESS_SECRET: z.string().min(16).default(DEFAULT_JWT_SECRET),
  JWT_ACCESS_TTL_SECONDS: positive_int.default(900),
  JWT_GUEST_TTL_SECONDS: positive_int.default(43200),
  REFRESH_TOKEN_TTL_DAYS: positive_int.default(30),
  CORS_ORIGINS: z.string().default("http://localhost:3000"),
  RATE_LIMIT_WINDOW_SECONDS: positive_int.default(60),
  RATE_LIMIT_MAX: positive_int.default(120),
  AUTH_RATE_LIMIT_MAX: positive_int.default(10),
  LOBBY_CODE_LENGTH: positive_int.min(4).max(12).default(5),
  LOBBY_COUNTDOWN_SECONDS: positive_int.default(5),
  LOBBY_EMPTY_DISPOSE_SECONDS: positive_int.default(120),
  LOBBY_RECONNECT_SECONDS: positive_int.default(20),
  DEFAULT_GAME_TYPE: z.string().min(1).default("placeholder_game"),
  GAME_INPUT_RATE_PER_SECOND: positive_int.default(60),
  CHAT_MAX_LENGTH: positive_int.default(200),
  CHAT_RATE_MESSAGES: positive_int.default(5),
  CHAT_RATE_WINDOW_SECONDS: positive_int.default(10),
  CHAT_HISTORY_SIZE: positive_int.default(30),
  FRIENDS_MAX: positive_int.default(100),
  INVITE_TTL_SECONDS: positive_int.default(300),
  MONITOR_USER: z.string().min(1).default("admin"),
  MONITOR_PASSWORD: z.string().min(1).default(DEFAULT_MONITOR_PASSWORD)
})

export type Environment = z.infer<typeof environment_schema>

function load_environment(): Environment {
  const parsed = environment_schema.safeParse(process.env)
  if (!parsed.success) {
    const problems = parsed.error.issues.map((issue) => `${issue.path.join(".")}: ${issue.message}`)
    throw new Error(`invalid environment: ${problems.join("; ")}`)
  }
  const values = parsed.data
  if (values.NODE_ENV === "production") {
    const unsafe: string[] = []
    if (values.JWT_ACCESS_SECRET === DEFAULT_JWT_SECRET) unsafe.push("JWT_ACCESS_SECRET")
    if (values.MONITOR_PASSWORD === DEFAULT_MONITOR_PASSWORD) unsafe.push("MONITOR_PASSWORD")
    if (unsafe.length > 0) {
      throw new Error(`refusing to start in production with default values for: ${unsafe.join(", ")}`)
    }
  }
  return values
}

function load_environment_or_exit(): Environment {
  try {
    return load_environment()
  } catch (error) {
    console.error(JSON.stringify({ level: 60, msg: (error as Error).message }))
    process.exit(1)
  }
}

export const environment = load_environment_or_exit()
