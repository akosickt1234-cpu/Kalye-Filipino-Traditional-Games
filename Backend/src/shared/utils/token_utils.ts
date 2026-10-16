import jwt from "jsonwebtoken"
import { z } from "zod"
import { authentication_config } from "../../config/authentication_config"
import { AppError } from "../../core/errors/app_error"
import { ERROR_CODES } from "../constants/error_codes"
import type { AccessTokenPayload, PlayerIdentity } from "../types/identity"

const payload_schema = z.object({
  sub: z.string().min(1),
  username: z.string().min(1),
  is_guest: z.boolean(),
  avatar: z.string().min(1),
  iat: z.number(),
  exp: z.number()
})

export function sign_access_token(identity: PlayerIdentity): string {
  const ttl_seconds = identity.is_guest
    ? authentication_config.jwt_guest_ttl_seconds
    : authentication_config.jwt_access_ttl_seconds
  return jwt.sign(
    {
      username: identity.username,
      is_guest: identity.is_guest,
      avatar: identity.avatar
    },
    authentication_config.jwt_access_secret,
    { algorithm: "HS256", subject: identity.id, expiresIn: ttl_seconds }
  )
}

export function verify_access_token(token: string): AccessTokenPayload {
  let decoded: unknown
  try {
    decoded = jwt.verify(token, authentication_config.jwt_access_secret, { algorithms: ["HS256"] })
  } catch (error) {
    if (error instanceof jwt.TokenExpiredError) {
      throw new AppError(ERROR_CODES.TOKEN_EXPIRED)
    }
    throw new AppError(ERROR_CODES.INVALID_TOKEN)
  }
  const parsed = payload_schema.safeParse(decoded)
  if (!parsed.success) throw new AppError(ERROR_CODES.INVALID_TOKEN)
  return parsed.data
}

export function payload_to_identity(payload: AccessTokenPayload): PlayerIdentity {
  return {
    id: payload.sub,
    username: payload.username,
    is_guest: payload.is_guest,
    avatar: payload.avatar
  }
}

export function extract_bearer_token(header_value: string | undefined): string | null {
  if (!header_value) return null
  const [scheme, value] = header_value.split(" ")
  if (!scheme || scheme.toLowerCase() !== "bearer" || !value) return null
  return value
}
