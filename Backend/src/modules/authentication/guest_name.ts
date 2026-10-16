import { AppError } from "../../core/errors/app_error"
import { ERROR_CODES } from "../../shared/constants/error_codes"

const GUEST_NAME_PATTERN = /^[\p{L}\p{N} _-]+$/u
const GUEST_NAME_MIN = 3
const GUEST_NAME_MAX = 20

export function normalize_guest_name(raw: string | undefined): string {
  const name = (raw ?? "").trim()
  if (name.length === 0) {
    throw new AppError(ERROR_CODES.GUEST_NAME_INVALID, "guest name is required")
  }
  const length = [...name].length
  if (length < GUEST_NAME_MIN || length > GUEST_NAME_MAX) {
    throw new AppError(
      ERROR_CODES.GUEST_NAME_INVALID,
      `guest name must be ${GUEST_NAME_MIN} to ${GUEST_NAME_MAX} characters`
    )
  }
  if (!GUEST_NAME_PATTERN.test(name)) {
    throw new AppError(
      ERROR_CODES.GUEST_NAME_INVALID,
      "guest name can only use letters, digits, space, underscore and hyphen"
    )
  }
  if (name.includes("  ")) {
    throw new AppError(ERROR_CODES.GUEST_NAME_INVALID, "guest name cannot have double spaces")
  }
  return name
}
