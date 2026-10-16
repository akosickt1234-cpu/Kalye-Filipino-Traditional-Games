import type { NextFunction, Request, Response } from "express"
import { rate_limit_config } from "../../config/rate_limit_config"
import { AppError } from "../../core/errors/app_error"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { create_rate_limiter } from "../../shared/utils/rate_limiter"

const limiter = create_rate_limiter(
  rate_limit_config.auth_max_requests,
  rate_limit_config.window_seconds * 1000
)

export function authentication_rate_limit(req: Request, _res: Response, next: NextFunction): void {
  if (req.path !== "/auth" && !req.path.startsWith("/auth/")) {
    next()
    return
  }
  if (limiter.allow(req.ip ?? "unknown")) {
    next()
    return
  }
  next(new AppError(ERROR_CODES.RATE_LIMITED))
}
