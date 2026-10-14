import type { NextFunction, Request, Response } from "express"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { extract_bearer_token, payload_to_identity, verify_access_token } from "../../shared/utils/token_utils"
import { AppError } from "../errors/app_error"

function authenticate(req: Request): void {
  const token = extract_bearer_token(req.headers.authorization)
  if (!token) throw new AppError(ERROR_CODES.UNAUTHENTICATED)
  req.player = payload_to_identity(verify_access_token(token))
}

export function require_auth(req: Request, _res: Response, next: NextFunction): void {
  try {
    authenticate(req)
    next()
  } catch (error) {
    next(error)
  }
}

export function require_registered(req: Request, _res: Response, next: NextFunction): void {
  try {
    authenticate(req)
    if (req.player?.is_guest) {
      throw new AppError(ERROR_CODES.FORBIDDEN, "guests cannot use this feature")
    }
    next()
  } catch (error) {
    next(error)
  }
}
