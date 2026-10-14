import { randomUUID } from "node:crypto"
import argon2 from "argon2"
import { AppError } from "../../core/errors/app_error"
import { query, with_transaction } from "../../infrastructure/database/database_client"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import type { PlayerIdentity } from "../../shared/types/identity"
import { sign_access_token } from "../../shared/utils/token_utils"
import type { GuestInput, LoginInput, RegisterInput, UpdateMeInput } from "./authentication_schemas"
import { normalize_guest_name } from "./guest_name"
import {
  hash_refresh_token,
  revoke_refresh_token,
  store_refresh_token
} from "./refresh_token_store"

const DEFAULT_AVATAR = "boy1"

interface UserRow {
  id: string
  username: string
  email: string
  password_hash: string
  avatar: string
}

export interface PublicUser {
  id: string
  username: string
  is_guest: boolean
  avatar: string
  email?: string
}

export interface RegisteredSession {
  access_token: string
  refresh_token: string
  user: PublicUser
}

export interface GuestSession {
  access_token: string
  user: PublicUser
}

const USER_COLUMNS = "id, username, email, password_hash, avatar"

let dummy_hash: Promise<string> | null = null

function get_dummy_hash(): Promise<string> {
  if (!dummy_hash) dummy_hash = argon2.hash(randomUUID())
  return dummy_hash
}

function to_identity(user: UserRow): PlayerIdentity {
  return { id: user.id, username: user.username, is_guest: false, avatar: user.avatar }
}

function to_public_user(user: UserRow): PublicUser {
  return {
    id: user.id,
    username: user.username,
    is_guest: false,
    avatar: user.avatar,
    email: user.email
  }
}

function map_unique_violation(error: unknown): never {
  const candidate = error as { code?: string; constraint?: string }
  if (candidate?.code === "23505") {
    const constraint = candidate.constraint ?? ""
    if (constraint.includes("email")) throw new AppError(ERROR_CODES.EMAIL_TAKEN)
    if (constraint.includes("username")) throw new AppError(ERROR_CODES.USERNAME_TAKEN)
    throw new AppError(ERROR_CODES.CONFLICT)
  }
  throw error
}

async function start_session(user: UserRow): Promise<RegisteredSession> {
  const refresh_token = await store_refresh_token(user.id)
  return {
    access_token: sign_access_token(to_identity(user)),
    refresh_token,
    user: to_public_user(user)
  }
}

export async function register_user(input: RegisterInput): Promise<RegisteredSession> {
  const password_hash = await argon2.hash(input.password)
  let user: UserRow
  try {
    const result = await query<UserRow>(
      `insert into users (username, email, password_hash) values ($1, $2, $3) returning ${USER_COLUMNS}`,
      [input.username, input.email, password_hash]
    )
    user = result.rows[0]
  } catch (error) {
    map_unique_violation(error)
  }
  return start_session(user)
}

export async function login_user(input: LoginInput): Promise<RegisteredSession> {
  const result = await query<UserRow>(
    `select ${USER_COLUMNS} from users where username = $1 or email = $1 limit 1`,
    [input.identifier]
  )
  const user = result.rows[0]
  const valid = await argon2
    .verify(user ? user.password_hash : await get_dummy_hash(), input.password)
    .catch(() => false)
  if (!user || !valid) throw new AppError(ERROR_CODES.INVALID_CREDENTIALS)
  return start_session(user)
}

export async function create_guest(input: GuestInput): Promise<GuestSession> {
  const username = normalize_guest_name(input.username)
  const taken = await query("select 1 from users where username = $1", [username])
  if ((taken.rowCount ?? 0) > 0) {
    throw new AppError(ERROR_CODES.USERNAME_TAKEN, "name matches a registered username")
  }
  const identity: PlayerIdentity = {
    id: `guest_${randomUUID()}`,
    username,
    is_guest: true,
    avatar: input.avatar ?? DEFAULT_AVATAR
  }
  return {
    access_token: sign_access_token(identity),
    user: {
      id: identity.id,
      username: identity.username,
      is_guest: true,
      avatar: identity.avatar
    }
  }
}

export async function rotate_refresh_token(token: string): Promise<RegisteredSession> {
  return with_transaction(async (client) => {
    const found = await client.query<UserRow>(
      `select u.id, u.username, u.email, u.password_hash, u.avatar
       from refresh_tokens rt
       join users u on u.id = rt.user_id
       where rt.token_hash = $1 and rt.revoked_at is null and rt.expires_at > now()
       for update of rt`,
      [hash_refresh_token(token)]
    )
    const user = found.rows[0]
    if (!user) throw new AppError(ERROR_CODES.INVALID_TOKEN, "refresh token is invalid")
    await client.query(
      "update refresh_tokens set revoked_at = now() where token_hash = $1",
      [hash_refresh_token(token)]
    )
    const refresh_token = await store_refresh_token(user.id, client)
    return {
      access_token: sign_access_token(to_identity(user)),
      refresh_token,
      user: to_public_user(user)
    }
  })
}

export async function logout_user(token: string): Promise<void> {
  await revoke_refresh_token(token)
}

export async function get_me(identity: PlayerIdentity): Promise<PublicUser> {
  if (identity.is_guest) {
    return {
      id: identity.id,
      username: identity.username,
      is_guest: true,
      avatar: identity.avatar
    }
  }
  const result = await query<UserRow>(`select ${USER_COLUMNS} from users where id = $1`, [identity.id])
  const user = result.rows[0]
  if (!user) throw new AppError(ERROR_CODES.NOT_FOUND, "account not found")
  return to_public_user(user)
}

export async function update_me(
  identity: PlayerIdentity,
  input: UpdateMeInput
): Promise<{ access_token: string; user: PublicUser }> {
  let user: UserRow | undefined
  try {
    const result = await query<UserRow>(
      `update users
       set username = coalesce($2, username),
           avatar = coalesce($3, avatar),
           updated_at = now()
       where id = $1
       returning ${USER_COLUMNS}`,
      [identity.id, input.username ?? null, input.avatar ?? null]
    )
    user = result.rows[0]
  } catch (error) {
    map_unique_violation(error)
  }
  if (!user) throw new AppError(ERROR_CODES.NOT_FOUND, "account not found")
  return { access_token: sign_access_token(to_identity(user)), user: to_public_user(user) }
}
