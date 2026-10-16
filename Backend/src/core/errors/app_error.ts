import { ERROR_CODES, type ErrorCode } from "../../shared/constants/error_codes"

const ERROR_STATUS: Record<ErrorCode, number> = {
  VALIDATION_FAILED: 400,
  UNAUTHENTICATED: 401,
  INVALID_TOKEN: 401,
  TOKEN_EXPIRED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  RATE_LIMITED: 429,
  INTERNAL_ERROR: 500,
  INVALID_CREDENTIALS: 401,
  USERNAME_TAKEN: 409,
  EMAIL_TAKEN: 409,
  GUEST_NAME_INVALID: 400,
  LOBBY_NOT_FOUND: 404,
  LOBBY_FULL: 409,
  LOBBY_IN_GAME: 409,
  NOT_HOST: 403,
  NOT_ENOUGH_PLAYERS: 409,
  GAME_NOT_FOUND: 404,
  ALREADY_FRIENDS: 409,
  REQUEST_PENDING: 409,
  INVITE_EXPIRED: 410,
  CHAT_RATE_LIMITED: 429,
  CHAT_TOO_LONG: 400
}

const DEFAULT_MESSAGES: Record<ErrorCode, string> = {
  VALIDATION_FAILED: "request data is invalid",
  UNAUTHENTICATED: "authentication is required",
  INVALID_TOKEN: "token is invalid",
  TOKEN_EXPIRED: "token has expired",
  FORBIDDEN: "you are not allowed to do this",
  NOT_FOUND: "resource not found",
  CONFLICT: "request conflicts with the current state",
  RATE_LIMITED: "too many requests, try again later",
  INTERNAL_ERROR: "something went wrong",
  INVALID_CREDENTIALS: "invalid credentials",
  USERNAME_TAKEN: "username is already taken",
  EMAIL_TAKEN: "email is already registered",
  GUEST_NAME_INVALID: "guest name is not allowed",
  LOBBY_NOT_FOUND: "lobby not found",
  LOBBY_FULL: "lobby is full",
  LOBBY_IN_GAME: "lobby is already in a game",
  NOT_HOST: "only the host can do this",
  NOT_ENOUGH_PLAYERS: "not enough players to start",
  GAME_NOT_FOUND: "game not found",
  ALREADY_FRIENDS: "you are already friends",
  REQUEST_PENDING: "a request is already pending",
  INVITE_EXPIRED: "invite has expired",
  CHAT_RATE_LIMITED: "you are sending messages too fast",
  CHAT_TOO_LONG: "message is too long"
}

export interface ErrorBody {
  error: {
    code: ErrorCode
    message: string
    details: Record<string, unknown>
  }
}

export class AppError extends Error {
  readonly code: ErrorCode
  readonly status: number
  readonly details: Record<string, unknown>

  constructor(code: ErrorCode, message?: string, details: Record<string, unknown> = {}) {
    super(message ?? DEFAULT_MESSAGES[code])
    this.name = "AppError"
    this.code = code
    this.status = ERROR_STATUS[code]
    this.details = details
  }
}

export function status_for_code(code: ErrorCode): number {
  return ERROR_STATUS[code]
}

export function default_message_for_code(code: ErrorCode): string {
  return DEFAULT_MESSAGES[code]
}

export function to_error_body(
  code: ErrorCode,
  message: string,
  details: Record<string, unknown> = {}
): ErrorBody {
  return { error: { code, message, details } }
}

export function is_app_error(value: unknown): value is AppError {
  return value instanceof AppError
}

export const INTERNAL_ERROR_BODY: ErrorBody = to_error_body(
  ERROR_CODES.INTERNAL_ERROR,
  DEFAULT_MESSAGES.INTERNAL_ERROR
)
