import { createHash, randomBytes } from "node:crypto"
import type { PoolClient } from "pg"
import { authentication_config } from "../../config/authentication_config"
import { query } from "../../infrastructure/database/database_client"

const DAY_MS = 24 * 60 * 60 * 1000

export function hash_refresh_token(token: string): string {
  return createHash("sha256").update(token).digest("hex")
}

export function generate_refresh_token(): string {
  return randomBytes(32).toString("base64url")
}

export async function store_refresh_token(
  user_id: string,
  executor?: PoolClient
): Promise<string> {
  const token = generate_refresh_token()
  const expires_at = new Date(Date.now() + authentication_config.refresh_token_ttl_days * DAY_MS)
  const sql =
    "insert into refresh_tokens (user_id, token_hash, expires_at) values ($1, $2, $3)"
  const params = [user_id, hash_refresh_token(token), expires_at]
  if (executor) await executor.query(sql, params)
  else await query(sql, params)
  return token
}

export async function revoke_refresh_token(token: string): Promise<void> {
  await query(
    "update refresh_tokens set revoked_at = now() where token_hash = $1 and revoked_at is null",
    [hash_refresh_token(token)]
  )
}

export async function delete_expired_refresh_tokens(): Promise<number> {
  const result = await query("delete from refresh_tokens where expires_at < now()")
  return result.rowCount ?? 0
}
