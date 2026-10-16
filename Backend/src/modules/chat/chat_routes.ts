import type { Express } from "express"
import { application_config } from "../../config/application_config"
import { chat_config } from "../../config/chat_config"
import { parse_input } from "../../core/errors/parse_input"
import { AppError } from "../../core/errors/app_error"
import { require_auth } from "../../core/middleware/authentication_middleware"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { async_handler } from "../../shared/utils/async_handler"
import { code_param_schema, poll_query_schema, send_body_schema } from "./chat_schemas"
import { add_message, allow_send, check_membership, read_messages } from "./chat_store"
import { filter_text } from "./text_filter"

const CONTROL_CHARACTERS = /[\u0000-\u001f\u007f-\u009f]/g

function clean_text(raw: string): string {
  return raw.replace(CONTROL_CHARACTERS, "").trim()
}

export function register_chat_routes(app: Express): void {
  app.post(
    "/chat/:code",
    require_auth,
    async_handler(async (req, res) => {
      const { code } = parse_input(code_param_schema, req.params)
      const player = req.player!
      check_membership(code, player.id)
      const { text: raw_text } = parse_input(send_body_schema, req.body ?? {})
      const text = clean_text(raw_text)
      if (text.length === 0) throw new AppError(ERROR_CODES.VALIDATION_FAILED, "message text is empty")
      if ([...text].length > chat_config.max_length) throw new AppError(ERROR_CODES.CHAT_TOO_LONG)
      allow_send(player.id)
      const message = add_message(code, player.id, player.username, filter_text(text))
      res.status(201).json(message)
    })
  )

  app.get(
    "/chat/:code",
    require_auth,
    async_handler(async (req, res) => {
      const { code } = parse_input(code_param_schema, req.params)
      const { after, wait } = parse_input(poll_query_schema, req.query)
      const wait_seconds = Math.min(wait ?? 0, application_config.long_poll_max_seconds)
      const messages = await read_messages(code, req.player!.id, after, wait_seconds, res)
      if (messages === null) return
      res.json({ messages })
    })
  )
}
