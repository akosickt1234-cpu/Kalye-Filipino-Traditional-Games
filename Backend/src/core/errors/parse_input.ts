import type { ZodTypeAny, z } from "zod"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { AppError } from "./app_error"

export function parse_input<T extends ZodTypeAny>(schema: T, data: unknown): z.infer<T> {
  const result = schema.safeParse(data)
  if (result.success) return result.data
  const fields = result.error.issues.map((issue) => ({
    path: issue.path.join("."),
    message: issue.message
  }))
  throw new AppError(ERROR_CODES.VALIDATION_FAILED, undefined, { fields })
}
