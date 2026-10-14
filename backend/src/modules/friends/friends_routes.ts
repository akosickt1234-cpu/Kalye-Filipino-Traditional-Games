import { Router } from "express"
import { parse_input } from "../../core/errors/parse_input"
import { require_registered } from "../../core/middleware/authentication_middleware"
import { async_handler } from "../../shared/utils/async_handler"
import {
  create_invite_schema,
  invite_id_param_schema,
  search_query_schema,
  user_id_body_schema,
  user_id_param_schema
} from "./friends_schemas"
import {
  accept_request,
  decline_request,
  list_friends,
  list_requests,
  remove_friendship,
  search_users,
  send_request
} from "./friends_service"
import { create_invite, decline_invite, list_invites } from "./invite_service"
import { record_heartbeat } from "./presence_service"

export function create_friends_router(): Router {
  const router = Router()
  router.use(require_registered)

  router.get(
    "/",
    async_handler(async (req, res) => {
      res.json({ friends: await list_friends(req.player!.id) })
    })
  )

  router.get(
    "/requests",
    async_handler(async (req, res) => {
      res.json(await list_requests(req.player!.id))
    })
  )

  router.get(
    "/search",
    async_handler(async (req, res) => {
      const { q } = parse_input(search_query_schema, req.query)
      res.json({ users: await search_users(req.player!.id, q) })
    })
  )

  router.post(
    "/request",
    async_handler(async (req, res) => {
      const { user_id } = parse_input(user_id_body_schema, req.body)
      const result = await send_request(req.player!.id, user_id)
      res.status(result.status === "pending" ? 201 : 200).json(result)
    })
  )

  router.post(
    "/accept",
    async_handler(async (req, res) => {
      const { user_id } = parse_input(user_id_body_schema, req.body)
      await accept_request(req.player!.id, user_id)
      res.status(204).end()
    })
  )

  router.post(
    "/decline",
    async_handler(async (req, res) => {
      const { user_id } = parse_input(user_id_body_schema, req.body)
      await decline_request(req.player!.id, user_id)
      res.status(204).end()
    })
  )

  router.delete(
    "/:user_id",
    async_handler(async (req, res) => {
      const { user_id } = parse_input(user_id_param_schema, req.params)
      await remove_friendship(req.player!.id, user_id)
      res.status(204).end()
    })
  )

  return router
}

export function create_invites_router(): Router {
  const router = Router()
  router.use(require_registered)

  router.post(
    "/",
    async_handler(async (req, res) => {
      const body = parse_input(create_invite_schema, req.body)
      res.status(201).json(await create_invite(req.player!.id, body.to_user_id, body.lobby_code))
    })
  )

  router.get(
    "/",
    async_handler(async (req, res) => {
      res.json({ invites: await list_invites(req.player!.id) })
    })
  )

  router.post(
    "/:id/decline",
    async_handler(async (req, res) => {
      const { id } = parse_input(invite_id_param_schema, req.params)
      await decline_invite(req.player!.id, id)
      res.status(204).end()
    })
  )

  return router
}

export function create_presence_router(): Router {
  const router = Router()
  router.use(require_registered)

  router.post("/heartbeat", (req, res) => {
    record_heartbeat(req.player!.id)
    res.status(204).end()
  })

  return router
}
