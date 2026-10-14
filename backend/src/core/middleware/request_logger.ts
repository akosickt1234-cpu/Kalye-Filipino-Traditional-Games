import type { NextFunction, Request, Response } from "express"
import { child_logger } from "../logging/logger"

const log = child_logger("http")

export function request_logger(req: Request, res: Response, next: NextFunction): void {
  const started_at = process.hrtime.bigint()
  res.on("finish", () => {
    const duration_ms = Number(process.hrtime.bigint() - started_at) / 1_000_000
    log.info(
      { method: req.method, path: req.path, status: res.statusCode, duration_ms: Math.round(duration_ms) },
      "request"
    )
  })
  next()
}
