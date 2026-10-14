# Phase 1: foundation

Branch: `backend_foundation`. Base: `develop`. Plan version: v0.6.

The backend is REST only. In-match multiplayer runs on Unity netcode. Lobby updates and lobby chat use long polling (section 7.5 of the plan).

## What is built

- Project files: `package.json` (version `0.1.0`, every dependency for all phases), `tsconfig.json`, `docker-compose.yml` (PostgreSQL 16 only), `.env.example`, `.gitignore`.
- `server.ts` and `app.ts`: plain Express app on a Node http server. Graceful shutdown stops jobs, closes the http server, calls each module `on_stop` in reverse order, then closes the database pool.
- Module loader, game loader, job runner, event bus, token utils, auth middleware, error classes, error middleware, request logger, logger, constants, types, rate limiter, code utils, config files.
- Database client, migration runner, `0001_users.sql`.
- `GET /health`.
- `games/placeholder_game/` with `index.ts` and `game_config.ts` (config only, 2 to 6 players, 1 team, display name marked as a placeholder).
- `docs/module_guide.md`: how to add a module, the long polling rules, how to add a game.

## Rework from the realtime version

The first version of this branch used Colyseus rooms over WebSocket. The rework removed:

- Dependencies `@colyseus/core`, `@colyseus/monitor`, `@colyseus/schema`, `@colyseus/ws-transport` and `colyseus.js`. The old Colyseus version pins are gone. `package-lock.json` was regenerated.
- The Colyseus server and WebSocket transport in `server.ts`.
- `rooms` and `room_features` from `ModuleDefinition`, and `get_room_features` and `attach_room_features` from the module loader.
- Files `core/rooms/base_room.ts`, `shared/schemas/base_game_state.ts`, `shared/constants/message_names.ts`, `shared/constants/room_names.ts`, `shared/constants/game_status.ts`, `games/placeholder_game/game_logic.ts`.
- `GameResultEntry`, and all game fields except `display_name`, `min_players`, `max_players`, `team_count`. `GameModule` is now `{ id, config }`. `MatchOutcome` and `MatchResult` stay.
- Error code `NOT_ALL_READY`. `CHAT_RATE_LIMITED` and `CHAT_TOO_LONG` stay.
- Env vars `WS_MAX_PAYLOAD_BYTES`, `LOBBY_COUNTDOWN_SECONDS`, `LOBBY_EMPTY_DISPOSE_SECONDS`, `LOBBY_RECONNECT_SECONDS`, `GAME_INPUT_RATE_PER_SECOND`, `MONITOR_USER`, `MONITOR_PASSWORD`.
- `experimentalDecorators` and `useDefineForClassFields` from `tsconfig.json`.

Added in the rework: env vars `LOBBY_HEARTBEAT_TIMEOUT_SECONDS`, `PRESENCE_TIMEOUT_SECONDS`, `LONG_POLL_MAX_SECONDS`, and `config/friends_config.ts`.

## Routes added

- `GET /health` returns `status`, `uptime`, `db`. HTTP 200 and `"ok"` when the database is up, HTTP 503 and `"degraded"` when it is down.

## Env vars used

All variables from section 7.7 are defined in `.env.example` and validated in `config/environment.ts`. In production the server refuses to start while `JWT_ACCESS_SECRET` is still the default.

Config slices:

- `application_config.ts`: `node_env`, `is_production`, `port`, `log_level`, `cors_origins`, `long_poll_max_seconds` (shared by lobby and chat).
- `authentication_config.ts`: JWT secret and TTLs, refresh token TTL.
- `database_config.ts`: `database_url`.
- `rate_limit_config.ts`: `window_seconds`, `max_requests`, `auth_max_requests`.
- `lobby_config.ts`: `code_length`, `heartbeat_timeout_seconds`, `default_game_type`.
- `friends_config.ts`: `friends_max`, `invite_ttl_seconds`, `presence_timeout_seconds`.
- `chat_config.ts`: `max_length`, `rate_messages`, `rate_window_seconds`, `history_size`.

## Shared resources other agents depend on

- `shared/constants/error_codes.ts`, `event_names.ts`, `avatar_ids.ts`
- `shared/types/module_types.ts`, `game_types.ts`, `identity.ts`, `event_types.ts`, `express.ts`
- `shared/utils/token_utils.ts`, `rate_limiter.ts`, `code_utils.ts`, `async_handler.ts`
- `core/events/event_bus.ts`
- `core/middleware/authentication_middleware.ts` (`require_auth`, `require_registered`), `error_middleware.ts`, `request_logger.ts`
- `core/errors/app_error.ts`, `parse_input.ts`
- `core/loaders/module_loader.ts`, `game_loader.ts` (`game_registry.get`, `game_registry.list`)
- `core/lifecycle/job_runner.ts`, `health.ts`
- `core/logging/logger.ts`
- `config/*`, `.env.example`
- `infrastructure/database/database_client.ts`, `migration_runner.ts`, `migrations/0001_users.sql`
- `games/placeholder_game/`
- `package.json`, `package-lock.json`, `tsconfig.json`

## Additions beyond the contract

Nothing in section 7 was renamed. Extra helpers other phases may use:

- `core/errors/parse_input.ts`: `parse_input(schema, data)` throws `VALIDATION_FAILED`.
- `shared/utils/async_handler.ts`: express 4 does not catch async errors.
- `shared/utils/token_utils.ts` also exports `payload_to_identity` and `extract_bearer_token`.
- `event_bus.on` returns an unsubscribe function. Handler errors are logged and never reach the emitter.

## Decisions to review

- `app.ts` registers a request logger (order 10) and `express.json` with a 10kb limit (order 15) so every module can read `req.body` on its own branch. Hardening also lists a 10kb JSON limit. If it adds its own parser it is a harmless second pass, since the body is already parsed.
- `GET /health` returning 503 when the database is down is a small addition to the plan.
- `package.json` has no `"type": "module"`. The build is CommonJS, and the loaders use `require`.
- Shutdown calls `closeIdleConnections()` after `http_server.close()`. Held long poll requests are not idle, so they must end on their own or at the 15 second forced exit. Lobby and chat phases should release held requests in `on_stop`.

## Verified

Run after the rework, on Linux with a local PostgreSQL 16 (user `kftg`, password `kftg`, database `kftg`):

- `npm run typecheck` and `npm run build` pass.
- `npm run migrate` applies `0001_users.sql`, and a second run reports the database is up to date.
- `node dist/server.js` and `npm run dev` start. `GET /health` returns `ok` with `db` up.
- A module folder with only an `index.ts` exporting `{ name }` is picked up by the loader. A second throwaway module with routes, a job, `on_start` and `on_stop` also loaded.
- `require_auth` and `require_registered`: no token gives `UNAUTHENTICATED` (401), a bad token `INVALID_TOKEN` (401), a guest on a registered route `FORBIDDEN` (403), a registered user passes.
- An unexpected thrown error returns `INTERNAL_ERROR` (500) with empty details and no internals. An unknown route returns `NOT_FOUND` in the same shape.
- SIGTERM on `dist/server.js` ran the job, then `on_stop`, and logged `shutdown complete`.
- Throwaway modules and scripts were removed afterwards.
- A grep of `package.json`, `package-lock.json`, `src`, `docs`, `.env.example`, `docker-compose.yml` and `tsconfig.json` finds no colyseus, websocket, room, monitor or `ws` reference, except in this file.

## Not verified

- `docker compose up -d`. The sandbox had no Docker daemon.
- Windows paths. The migration runner and loaders use `path`, but only Linux was run.
- Behavior of `npm run dev` on SIGTERM. Only the `/health` response was checked there.
- Production mode refusal with the default `JWT_ACCESS_SECRET`, token expiry, rate limiter and code utils were checked in the first version of this branch and not re-run after the rework. Their code was not changed.
- The only checks of held requests were by reasoning. No held request exists in this phase.
