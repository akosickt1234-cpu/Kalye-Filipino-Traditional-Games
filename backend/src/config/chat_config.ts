import { environment } from "./environment"

export const chat_config = {
  max_length: environment.CHAT_MAX_LENGTH,
  rate_messages: environment.CHAT_RATE_MESSAGES,
  rate_window_seconds: environment.CHAT_RATE_WINDOW_SECONDS,
  history_size: environment.CHAT_HISTORY_SIZE
} as const
