import { randomUUID } from "node:crypto"
import type { NextFunction, Request, Response } from "express"

const REQUEST_ID_HEADER = "X-Request-Id"
const ACCEPTED_ID_PATTERN = /^[A-Za-z0-9_-]{8,64}$/

export function request_id(req: Request, res: Response, next: NextFunction): void {
  const incoming = req.get(REQUEST_ID_HEADER)
  const id = incoming && ACCEPTED_ID_PATTERN.test(incoming) ? incoming : randomUUID()
  res.locals.request_id = id
  res.setHeader(REQUEST_ID_HEADER, id)
  next()
}
