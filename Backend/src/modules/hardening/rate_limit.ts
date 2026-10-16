import type { Request, Response } from "express"
import rateLimit from "express-rate-limit"
import { rate_limit_config } from "../../config/rate_limit_config"
import { default_message_for_code, to_error_body } from "../../core/errors/app_error"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { extract_bearer_token, verify_access_token } from "../../shared/utils/token_utils"

function limit_key(req: Request): string {
  const token = extract_bearer_token(req.headers.authorization)
  if (token) {
    try {
      return `player:${verify_access_token(token).sub}`
    } catch {
      return `ip:${req.ip ?? "unknown"}`
    }
  }
  return `ip:${req.ip ?? "unknown"}`
}

function limit_reached(_req: Request, res: Response): void {
  const retry_after_seconds = Number(res.getHeader("Retry-After")) || rate_limit_config.window_seconds
  res
    .status(429)
    .json(
      to_error_body(ERROR_CODES.RATE_LIMITED, default_message_for_code(ERROR_CODES.RATE_LIMITED), {
        retry_after_seconds
      })
    )
}

export const global_rate_limit = rateLimit({
  windowMs: rate_limit_config.window_seconds * 1000,
  limit: rate_limit_config.max_requests,
  keyGenerator: limit_key,
  standardHeaders: "draft-7",
  legacyHeaders: false,
  handler: limit_reached
})
