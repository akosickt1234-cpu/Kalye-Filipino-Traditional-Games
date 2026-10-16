import { query, with_transaction } from "../../infrastructure/database/database_client"
import type { MatchResult } from "../../shared/types/game_types"

const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

export interface MatchCursor {
  ended_at: string
  id: string
}

export interface MatchPlayerRow {
  player_id: string
  user_id: string | null
  is_guest: boolean
  username: string
  team: string | null
  result: "win" | "loss" | "draw"
  score: number
}

export interface UserMatchRow {
  match_id: string
  game_type: string
  lobby_code: string | null
  started_at: Date
  ended_at: Date
  team: string | null
  result: "win" | "loss" | "draw"
  score: number
  players: MatchPlayerRow[]
  cursor: MatchCursor
}

export interface StatsRow {
  game_type: string
  played: number
  wins: number
  losses: number
  draws: number
}

export interface LeaderboardRow {
  user_id: string
  username: string
  wins: number
  played: number
}

function clean_user_id(player: MatchResult["players"][number]): string | null {
  if (player.is_guest || !player.user_id) return null
  return UUID_PATTERN.test(player.user_id) ? player.user_id : null
}

export async function save_match(match: MatchResult): Promise<boolean> {
  return with_transaction(async (client) => {
    const inserted = await client.query(
      `insert into matches (id, game_type, lobby_code, started_at, ended_at)
       values ($1, $2, $3, $4, $5)
       on conflict (id) do nothing
       returning id`,
      [match.match_id, match.game_type, match.lobby_code, match.started_at, match.ended_at]
    )
    if (inserted.rowCount === 0) return false
    if (match.players.length === 0) return true
    await client.query(
      `insert into match_players (match_id, player_id, user_id, is_guest, username, team, result, score)
       select $1::uuid, * from unnest(
         $2::text[], $3::uuid[], $4::boolean[], $5::text[], $6::text[], $7::text[], $8::integer[]
       )
       on conflict (match_id, player_id) do nothing`,
      [
        match.match_id,
        match.players.map((player) => player.player_id),
        match.players.map(clean_user_id),
        match.players.map((player) => player.is_guest),
        match.players.map((player) => player.username),
        match.players.map((player) => player.team),
        match.players.map((player) => player.result),
        match.players.map((player) => player.score)
      ]
    )
    return true
  })
}

export async function list_user_matches(
  user_id: string,
  limit: number,
  cursor: MatchCursor | null
): Promise<{ rows: UserMatchRow[]; has_more: boolean }> {
  const page = await query<{
    id: string
    game_type: string
    lobby_code: string | null
    started_at: Date
    ended_at: Date
    ended_cursor: string
    team: string | null
    result: "win" | "loss" | "draw"
    score: number
  }>(
    `select m.id, m.game_type, m.lobby_code, m.started_at, m.ended_at,
            to_char(m.ended_at at time zone 'utc', 'YYYY-MM-DD"T"HH24:MI:SS.US"Z"') as ended_cursor,
            mp.team, mp.result, mp.score
     from match_players mp
     join matches m on m.id = mp.match_id
     where mp.user_id = $1
       and ($2::timestamptz is null or (m.ended_at, m.id) < ($2::timestamptz, $3::uuid))
     order by m.ended_at desc, m.id desc
     limit $4`,
    [user_id, cursor?.ended_at ?? null, cursor?.id ?? null, limit + 1]
  )
  const has_more = page.rows.length > limit
  const rows = page.rows.slice(0, limit)
  if (rows.length === 0) return { rows: [], has_more: false }
  const players = await query<MatchPlayerRow & { match_id: string }>(
    `select match_id, player_id, user_id, is_guest, username, team, result, score
     from match_players
     where match_id = any($1::uuid[])
     order by match_id, score desc, username`,
    [rows.map((row) => row.id)]
  )
  const by_match = new Map<string, MatchPlayerRow[]>()
  for (const { match_id, ...player } of players.rows) {
    const list = by_match.get(match_id) ?? []
    list.push(player)
    by_match.set(match_id, list)
  }
  return {
    has_more,
    rows: rows.map((row) => ({
      match_id: row.id,
      game_type: row.game_type,
      lobby_code: row.lobby_code,
      started_at: row.started_at,
      ended_at: row.ended_at,
      team: row.team,
      result: row.result,
      score: row.score,
      players: by_match.get(row.id) ?? [],
      cursor: { ended_at: row.ended_cursor, id: row.id }
    }))
  }
}

export async function get_user_stats(user_id: string): Promise<StatsRow[]> {
  const result = await query<StatsRow>(
    `select m.game_type,
            count(*)::int as played,
            (count(*) filter (where mp.result = 'win'))::int as wins,
            (count(*) filter (where mp.result = 'loss'))::int as losses,
            (count(*) filter (where mp.result = 'draw'))::int as draws
     from match_players mp
     join matches m on m.id = mp.match_id
     where mp.user_id = $1
     group by m.game_type
     order by m.game_type`,
    [user_id]
  )
  return result.rows
}

export async function get_leaderboard(game_type: string, limit: number): Promise<LeaderboardRow[]> {
  const result = await query<LeaderboardRow>(
    `select u.id as user_id, u.username::text as username,
            (count(*) filter (where mp.result = 'win'))::int as wins,
            count(*)::int as played
     from match_players mp
     join matches m on m.id = mp.match_id
     join users u on u.id = mp.user_id
     where m.game_type = $1 and mp.user_id is not null
     group by u.id, u.username
     order by wins desc, played asc, username asc
     limit $2`,
    [game_type, limit]
  )
  return result.rows
}
