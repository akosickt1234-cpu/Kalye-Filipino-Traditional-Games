import type { PoolClient } from "pg"
import { friends_config } from "../../config/friends_config"
import { AppError } from "../../core/errors/app_error"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { query, with_transaction } from "../../infrastructure/database/database_client"
import type { FriendRequestView, FriendView, RelationStatus, SearchResultView } from "./friends_types"
import { get_presence } from "./presence_service"

const SEARCH_LIMIT = 10

interface FriendshipRow {
  id: string
  requester_id: string
  addressee_id: string
  status: "pending" | "accepted"
}

function iso(value: Date | string): string {
  return new Date(value).toISOString()
}

async function find_friendship(
  client: Pick<PoolClient, "query">,
  user_a: string,
  user_b: string
): Promise<FriendshipRow | null> {
  const result = await client.query<FriendshipRow>(
    `select id, requester_id, addressee_id, status from friendships
     where least(requester_id, addressee_id) = least($1::uuid, $2::uuid)
       and greatest(requester_id, addressee_id) = greatest($1::uuid, $2::uuid)`,
    [user_a, user_b]
  )
  return result.rows[0] ?? null
}

async function count_friends(client: Pick<PoolClient, "query">, user_id: string): Promise<number> {
  const result = await client.query<{ total: number }>(
    `select count(*)::int as total from friendships
     where status = 'accepted' and (requester_id = $1 or addressee_id = $1)`,
    [user_id]
  )
  return result.rows[0]?.total ?? 0
}

function friend_limit_error(): AppError {
  return new AppError(ERROR_CODES.CONFLICT, "friend limit reached", { friends_max: friends_config.friends_max })
}

export async function list_friends(user_id: string): Promise<FriendView[]> {
  const result = await query<{ id: string; username: string; avatar: string }>(
    `select u.id, u.username, u.avatar
     from friendships f
     join users u on u.id = case when f.requester_id = $1 then f.addressee_id else f.requester_id end
     where f.status = 'accepted' and (f.requester_id = $1 or f.addressee_id = $1)
     order by u.username`,
    [user_id]
  )
  return result.rows.map((row) => ({ ...row, status: get_presence(row.id) }))
}

export async function list_requests(
  user_id: string
): Promise<{ incoming: FriendRequestView[]; outgoing: FriendRequestView[] }> {
  const incoming = await query<{ id: string; username: string; avatar: string; created_at: Date }>(
    `select u.id, u.username, u.avatar, f.created_at
     from friendships f join users u on u.id = f.requester_id
     where f.addressee_id = $1 and f.status = 'pending'
     order by f.created_at desc`,
    [user_id]
  )
  const outgoing = await query<{ id: string; username: string; avatar: string; created_at: Date }>(
    `select u.id, u.username, u.avatar, f.created_at
     from friendships f join users u on u.id = f.addressee_id
     where f.requester_id = $1 and f.status = 'pending'
     order by f.created_at desc`,
    [user_id]
  )
  const to_view = (row: { id: string; username: string; avatar: string; created_at: Date }): FriendRequestView => ({
    id: row.id,
    username: row.username,
    avatar: row.avatar,
    created_at: iso(row.created_at)
  })
  return { incoming: incoming.rows.map(to_view), outgoing: outgoing.rows.map(to_view) }
}

export async function search_users(user_id: string, prefix: string): Promise<SearchResultView[]> {
  const result = await query<{
    id: string
    username: string
    avatar: string
    status: "pending" | "accepted" | null
    requester_id: string | null
  }>(
    `select u.id, u.username, u.avatar, f.status, f.requester_id
     from users u
     left join friendships f
       on least(f.requester_id, f.addressee_id) = least(u.id, $1::uuid)
      and greatest(f.requester_id, f.addressee_id) = greatest(u.id, $1::uuid)
     where u.id <> $1 and u.username like $2
     order by u.username
     limit ${SEARCH_LIMIT}`,
    [user_id, `${prefix}%`]
  )
  return result.rows.map((row) => {
    let relation: RelationStatus = "none"
    if (row.status === "accepted") relation = "friends"
    else if (row.status === "pending") relation = row.requester_id === user_id ? "request_sent" : "request_received"
    return { id: row.id, username: row.username, avatar: row.avatar, relation }
  })
}

export async function send_request(
  user_id: string,
  target_id: string
): Promise<{ user_id: string; status: "pending" | "accepted" }> {
  if (user_id === target_id) throw new AppError(ERROR_CODES.VALIDATION_FAILED, "you cannot add yourself")
  return with_transaction(async (client) => {
    const target = await client.query(`select 1 from users where id = $1`, [target_id])
    if (target.rowCount === 0) throw new AppError(ERROR_CODES.NOT_FOUND, "user not found")

    const existing = await find_friendship(client, user_id, target_id)
    if (existing?.status === "accepted") throw new AppError(ERROR_CODES.ALREADY_FRIENDS)
    if (existing && existing.requester_id === user_id) throw new AppError(ERROR_CODES.REQUEST_PENDING)

    if ((await count_friends(client, user_id)) >= friends_config.friends_max) throw friend_limit_error()

    if (existing) {
      if ((await count_friends(client, target_id)) >= friends_config.friends_max) throw friend_limit_error()
      await client.query(`update friendships set status = 'accepted' where id = $1`, [existing.id])
      return { user_id: target_id, status: "accepted" as const }
    }

    try {
      await client.query(`insert into friendships (requester_id, addressee_id) values ($1, $2)`, [user_id, target_id])
    } catch (error) {
      if ((error as { code?: string }).code === "23505") throw new AppError(ERROR_CODES.REQUEST_PENDING)
      throw error
    }
    return { user_id: target_id, status: "pending" as const }
  })
}

export async function accept_request(user_id: string, requester_id: string): Promise<void> {
  await with_transaction(async (client) => {
    const existing = await find_friendship(client, user_id, requester_id)
    if (!existing || existing.status !== "pending" || existing.addressee_id !== user_id) {
      throw new AppError(ERROR_CODES.NOT_FOUND, "friend request not found")
    }
    if ((await count_friends(client, user_id)) >= friends_config.friends_max) throw friend_limit_error()
    if ((await count_friends(client, requester_id)) >= friends_config.friends_max) throw friend_limit_error()
    await client.query(`update friendships set status = 'accepted' where id = $1`, [existing.id])
  })
}

export async function decline_request(user_id: string, requester_id: string): Promise<void> {
  const result = await query(
    `delete from friendships where requester_id = $1 and addressee_id = $2 and status = 'pending'`,
    [requester_id, user_id]
  )
  if (result.rowCount === 0) throw new AppError(ERROR_CODES.NOT_FOUND, "friend request not found")
}

export async function remove_friendship(user_id: string, other_id: string): Promise<void> {
  await with_transaction(async (client) => {
    const result = await client.query(
      `delete from friendships
       where least(requester_id, addressee_id) = least($1::uuid, $2::uuid)
         and greatest(requester_id, addressee_id) = greatest($1::uuid, $2::uuid)`,
      [user_id, other_id]
    )
    if (result.rowCount === 0) throw new AppError(ERROR_CODES.NOT_FOUND, "friendship not found")
    await client.query(
      `delete from room_invites
       where (from_user_id = $1 and to_user_id = $2) or (from_user_id = $2 and to_user_id = $1)`,
      [user_id, other_id]
    )
  })
}

export async function are_friends(user_id: string, other_id: string): Promise<boolean> {
  const result = await query(
    `select 1 from friendships
     where status = 'accepted'
       and least(requester_id, addressee_id) = least($1::uuid, $2::uuid)
       and greatest(requester_id, addressee_id) = greatest($1::uuid, $2::uuid)`,
    [user_id, other_id]
  )
  return (result.rowCount ?? 0) > 0
}
