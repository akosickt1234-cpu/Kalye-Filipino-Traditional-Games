import type { Express } from "express"
import { AppError } from "../../core/errors/app_error"
import { parse_input } from "../../core/errors/parse_input"
import { require_auth, require_registered } from "../../core/middleware/authentication_middleware"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { async_handler } from "../../shared/utils/async_handler"
import { leaderboard_params, leaderboard_query, list_matches_query } from "./match_schemas"
import {
  get_leaderboard,
  get_user_stats,
  list_user_matches,
  type MatchCursor
} from "./match_repository"

function encode_cursor(cursor: MatchCursor): string {
  return Buffer.from(`${cursor.ended_at}|${cursor.id}`).toString("base64url")
}

function decode_cursor(value: string): MatchCursor {
  const [ended_at, id, extra] = Buffer.from(value, "base64url").toString("utf8").split("|")
  const valid =
    extra === undefined &&
    typeof ended_at === "string" &&
    typeof id === "string" &&
    !Number.isNaN(Date.parse(ended_at)) &&
    /^[0-9a-f-]{36}$/i.test(id)
  if (!valid) throw new AppError(ERROR_CODES.VALIDATION_FAILED, "cursor is invalid")
  return { ended_at, id }
}

export function register_match_routes(app: Express): void {
  app.get(
    "/me/matches",
    require_registered,
    async_handler(async (req, res) => {
      const { limit, cursor } = parse_input(list_matches_query, req.query)
      const page = await list_user_matches(req.player!.id, limit, cursor ? decode_cursor(cursor) : null)
      const last = page.rows[page.rows.length - 1]
      res.json({
        matches: page.rows.map(({ cursor: _cursor, ...match }) => match),
        next_cursor: page.has_more && last ? encode_cursor(last.cursor) : null
      })
    })
  )

  app.get(
    "/me/stats",
    require_registered,
    async_handler(async (req, res) => {
      const games = await get_user_stats(req.player!.id)
      const total = games.reduce(
        (sum, game) => ({
          played: sum.played + game.played,
          wins: sum.wins + game.wins,
          losses: sum.losses + game.losses,
          draws: sum.draws + game.draws
        }),
        { played: 0, wins: 0, losses: 0, draws: 0 }
      )
      res.json({ total, games })
    })
  )

  app.get(
    "/leaderboard/:game_type",
    require_auth,
    async_handler(async (req, res) => {
      const { game_type } = parse_input(leaderboard_params, req.params)
      const { limit } = parse_input(leaderboard_query, req.query)
      const rows = await get_leaderboard(game_type, limit)
      res.json({
        game_type,
        entries: rows.map((row, index) => ({ rank: index + 1, ...row }))
      })
    })
  )
}
