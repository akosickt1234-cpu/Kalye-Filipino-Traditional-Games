import type { Express } from "express"
import { parse_input } from "../../core/errors/parse_input"
import { require_auth, require_registered } from "../../core/middleware/authentication_middleware"
import { async_handler } from "../../shared/utils/async_handler"
import {
  guest_schema,
  login_schema,
  refresh_schema,
  register_schema,
  update_me_schema
} from "./authentication_schemas"
import {
  create_guest,
  get_me,
  login_user,
  logout_user,
  register_user,
  rotate_refresh_token,
  update_me
} from "./authentication_service"

export function register_authentication_routes(app: Express): void {
  app.post(
    "/auth/register",
    async_handler(async (req, res) => {
      const input = parse_input(register_schema, req.body)
      res.status(201).json(await register_user(input))
    })
  )

  app.post(
    "/auth/login",
    async_handler(async (req, res) => {
      const input = parse_input(login_schema, req.body)
      res.json(await login_user(input))
    })
  )

  app.post(
    "/auth/guest",
    async_handler(async (req, res) => {
      const input = parse_input(guest_schema, req.body)
      res.status(201).json(await create_guest(input))
    })
  )

  app.post(
    "/auth/refresh",
    async_handler(async (req, res) => {
      const input = parse_input(refresh_schema, req.body)
      res.json(await rotate_refresh_token(input.refresh_token))
    })
  )

  app.post(
    "/auth/logout",
    async_handler(async (req, res) => {
      const input = parse_input(refresh_schema, req.body)
      await logout_user(input.refresh_token)
      res.status(204).end()
    })
  )

  app.get(
    "/me",
    require_auth,
    async_handler(async (req, res) => {
      res.json({ user: await get_me(req.player!) })
    })
  )

  app.patch(
    "/me",
    require_registered,
    async_handler(async (req, res) => {
      const input = parse_input(update_me_schema, req.body)
      res.json(await update_me(req.player!, input))
    })
  )
}
