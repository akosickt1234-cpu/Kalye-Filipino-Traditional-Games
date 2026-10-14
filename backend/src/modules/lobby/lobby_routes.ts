import type { Express, Request } from "express"
import { AppError } from "../../core/errors/app_error"
import { parse_input } from "../../core/errors/parse_input"
import { require_auth } from "../../core/middleware/authentication_middleware"
import { ERROR_CODES } from "../../shared/constants/error_codes"
import { async_handler } from "../../shared/utils/async_handler"
import {
  create_lobby,
  find_lobby,
  hold_for_change,
  join_lobby,
  kick_member,
  leave_lobby,
  list_public,
  lobby_summary,
  require_member,
  submit_result,
  touch_member,
  update_lobby
} from "./lobby_service"
import {
  create_lobby_schema,
  heartbeat_query_schema,
  kick_schema,
  result_schema,
  update_lobby_schema
} from "./lobby_schemas"
import { member_view } from "./lobby_views"

function player_of(req: Request) {
  const player = req.player
  if (!player) throw new AppError(ERROR_CODES.UNAUTHENTICATED)
  return player
}

function code_of(req: Request): string {
  return String(req.params.code ?? "")
}

export function register_lobby_routes(app: Express): void {
  app.post(
    "/lobbies",
    require_auth,
    async_handler(async (req, res) => {
      const input = parse_input(create_lobby_schema, req.body)
      res.status(201).json(member_view(create_lobby(player_of(req), input)))
    })
  )

  app.get("/lobbies/public", require_auth, (_req, res) => {
    res.json({ lobbies: list_public() })
  })

  app.get("/lobbies/:code", require_auth, (req, res) => {
    res.json(lobby_summary(code_of(req)))
  })

  app.post(
    "/lobbies/:code/join",
    require_auth,
    async_handler(async (req, res) => {
      res.json(member_view(join_lobby(player_of(req), code_of(req))))
    })
  )

  app.post(
    "/lobbies/:code/leave",
    require_auth,
    async_handler(async (req, res) => {
      leave_lobby(player_of(req), code_of(req))
      res.status(204).end()
    })
  )

  app.post(
    "/lobbies/:code/heartbeat",
    require_auth,
    async_handler(async (req, res) => {
      const player = player_of(req)
      const lobby = find_lobby(code_of(req))
      require_member(lobby, player.id)
      touch_member(lobby, player.id)
      const { since, wait } = parse_input(heartbeat_query_schema, req.query)

      if (since === undefined || !wait || lobby.version > since) {
        res.json(member_view(lobby))
        return
      }

      const outcome = await hold_for_change(lobby, player.id, wait, res)
      if (outcome === "disconnected") return
      if (lobby.closed) throw new AppError(ERROR_CODES.LOBBY_NOT_FOUND)
      require_member(lobby, player.id)
      res.json(member_view(lobby))
    })
  )

  app.patch(
    "/lobbies/:code",
    require_auth,
    async_handler(async (req, res) => {
      const input = parse_input(update_lobby_schema, req.body)
      res.json(member_view(update_lobby(player_of(req), code_of(req), input)))
    })
  )

  app.post(
    "/lobbies/:code/kick",
    require_auth,
    async_handler(async (req, res) => {
      const input = parse_input(kick_schema, req.body)
      res.json(member_view(kick_member(player_of(req), code_of(req), input.player_id)))
    })
  )

  app.post(
    "/lobbies/:code/result",
    require_auth,
    async_handler(async (req, res) => {
      const input = parse_input(result_schema, req.body)
      res.status(201).json(member_view(submit_result(player_of(req), code_of(req), input)))
    })
  )
}
