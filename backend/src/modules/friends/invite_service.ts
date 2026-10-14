import { friends_config } from "../../config/friends_config"
import { AppError } from "../../core/errors/app_error"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { query } from "../../infrastructure/database/database_client"
import { are_friends } from "./friends_service"
import type { InviteView } from "./friends_types"
import { get_lobby_of, is_lobby_in_game } from "./presence_service"

interface InviteRow {
  id: string
  lobby_code: string
  created_at: Date
  expires_at: Date
  from_id: string
  from_username: string
  from_avatar: string
}

export async function create_invite(
  from_user_id: string,
  to_user_id: string,
  lobby_code: string
): Promise<{ id: string; to_user_id: string; lobby_code: string; expires_at: string }> {
  if (from_user_id === to_user_id) throw new AppError(ERROR_CODES.VALIDATION_FAILED, "you cannot invite yourself")
  if (!(await are_friends(from_user_id, to_user_id))) {
    throw new AppError(ERROR_CODES.FORBIDDEN, "you can only invite friends")
  }
  if (get_lobby_of(from_user_id) !== lobby_code) {
    throw new AppError(ERROR_CODES.FORBIDDEN, "you are not in that lobby")
  }
  if (is_lobby_in_game(lobby_code)) throw new AppError(ERROR_CODES.LOBBY_IN_GAME)
  if (get_lobby_of(to_user_id) === lobby_code) {
    throw new AppError(ERROR_CODES.CONFLICT, "that player is already in the lobby")
  }

  const result = await query<{ id: string; expires_at: Date }>(
    `insert into room_invites (from_user_id, to_user_id, lobby_code, expires_at)
     values ($1, $2, $3, now() + make_interval(secs => $4))
     on conflict (from_user_id, to_user_id, lobby_code) where status = 'pending'
     do update set expires_at = excluded.expires_at, created_at = now()
     returning id, expires_at`,
    [from_user_id, to_user_id, lobby_code, friends_config.invite_ttl_seconds]
  )
  const row = result.rows[0]
  return { id: row.id, to_user_id, lobby_code, expires_at: new Date(row.expires_at).toISOString() }
}

export async function list_invites(user_id: string): Promise<InviteView[]> {
  const result = await query<InviteRow>(
    `select i.id, i.lobby_code, i.created_at, i.expires_at,
            u.id as from_id, u.username as from_username, u.avatar as from_avatar
     from room_invites i join users u on u.id = i.from_user_id
     where i.to_user_id = $1 and i.status = 'pending' and i.expires_at > now()
     order by i.created_at desc`,
    [user_id]
  )
  return result.rows.map((row) => ({
    id: row.id,
    from: { id: row.from_id, username: row.from_username, avatar: row.from_avatar },
    lobby_code: row.lobby_code,
    created_at: new Date(row.created_at).toISOString(),
    expires_at: new Date(row.expires_at).toISOString()
  }))
}

export async function decline_invite(user_id: string, invite_id: string): Promise<void> {
  const found = await query<{ status: string; expires_at: Date }>(
    `select status, expires_at from room_invites where id = $1 and to_user_id = $2`,
    [invite_id, user_id]
  )
  const row = found.rows[0]
  if (!row || row.status !== "pending") throw new AppError(ERROR_CODES.NOT_FOUND, "invite not found")
  if (new Date(row.expires_at).getTime() <= Date.now()) throw new AppError(ERROR_CODES.INVITE_EXPIRED)
  await query(`update room_invites set status = 'declined' where id = $1`, [invite_id])
}

export async function delete_expired_invites(): Promise<void> {
  await query(`delete from room_invites where expires_at < now() - interval '1 hour'`)
}

export async function drop_invites_for_closed_lobby(code: string): Promise<void> {
  await query(`delete from room_invites where lobby_code = $1`, [code])
}

export async function mark_invites_accepted(code: string, user_id: string): Promise<void> {
  await query(
    `update room_invites set status = 'accepted'
     where lobby_code = $1 and to_user_id = $2 and status = 'pending'`,
    [code, user_id]
  )
}
