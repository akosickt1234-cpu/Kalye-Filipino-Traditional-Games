import type { NextFunction, Request, Response } from "express"
import { ZodError } from "zod"
import { ERROR_CODES, type ErrorCode } from "../../shared/constants/error_codes"
import { AppError, INTERNAL_ERROR_BODY, to_error_body } from "../errors/app_error"
import { child_logger } from "../logging/logger"

const log = child_logger("http")

interface HttpLikeError {
  status?: number
  statusCode?: number
  type?: string
}

export function not_found_handler(_req: Request, res: Response): void {
  res
    .status(404)
    .json(to_error_body(ERROR_CODES.NOT_FOUND, "route not found"))
}

function body_parser_error(error: unknown): { status: number; code: ErrorCode; message: string } | null {
  const candidate = error as HttpLikeError
  const status = candidate.status ?? candidate.statusCode
  if (typeof status !== "number" || status < 400 || status >= 500) return null
  if (candidate.type === "entity.too.large") {
    return { status: 413, code: ERROR_CODES.VALIDATION_FAILED, message: "request body is too large" }
  }
  if (candidate.type === "entity.parse.failed") {
    return { status: 400, code: ERROR_CODES.VALIDATION_FAILED, message: "request body is not valid json" }
  }
  if (typeof candidate.type === "string" && candidate.type.startsWith("encoding.")) {
    return { status: 400, code: ERROR_CODES.VALIDATION_FAILED, message: "request body could not be read" }
  }
  return null
}

export function error_handler(error: unknown, req: Request, res: Response, next: NextFunction): void {
  if (res.headersSent) {
    next(error)
    return
  }
  if (error instanceof AppError) {
    res.status(error.status).json(to_error_body(error.code, error.message, error.details))
    return
  }
  if (error instanceof ZodError) {
    const fields = error.issues.map((issue) => ({ path: issue.path.join("."), message: issue.message }))
    res
      .status(400)
      .json(to_error_body(ERROR_CODES.VALIDATION_FAILED, "request data is invalid", { fields }))
    return
  }
  const parser_error = body_parser_error(error)
  if (parser_error) {
    res.status(parser_error.status).json(to_error_body(parser_error.code, parser_error.message))
    return
  }
  log.error(
    { err: error, request_id: res.locals.request_id, method: req.method, path: req.path },
    "unhandled request error"
  )
  res.status(500).json(INTERNAL_ERROR_BODY)
}
