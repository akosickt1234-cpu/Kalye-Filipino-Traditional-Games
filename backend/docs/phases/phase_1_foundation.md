# Phase 1: foundation

Branch: `backend_foundation`. Base: `develop`.

## What was built

- Project files: `package.json` (version `0.1.0`, every dependency for all phases), `tsconfig.json`, `docker-compose.yml` (PostgreSQL 16 only), `.env.example`, `.gitignore`.
- `server.ts` and `app.ts`: startup, graceful shutdown (stops jobs, shuts colyseus down, calls each module `on_stop` in reverse order, closes the database pool).
- Module loader, game loader, job runner, base room, event bus, token utils, auth middleware, error classes, error middleware, logger, constants, types, schemas, rate limiter, code utils, config files.
- Database client, migration runner, `0001_users.sql`.
- `GET /health`.
- `games/placeholder_game/` with `index.ts`, `game_config.ts` and a stub `game_logic.ts`.
- `docs/module_guide.md`.

## Routes and messages added

- `GET /health` returns `status`, `uptime`, `db`. HTTP 200 and `"ok"` when the database is up, HTTP 503 and `"degraded"` when it is down.
- No room messages. `BaseRoom` only adds `error` through `send_error`.

## Env vars used

All variables from section 7.8 are defined in `.env.example` and validated in `config/environment.ts`. In production the server refuses to start while `JWT_ACCESS_SECRET` or `MONITOR_PASSWORD` is still the default.

## Pinned versions

Colyseus server `@colyseus/core` 0.16.26, `@colyseus/schema` 3.0.76, `@colyseus/ws-transport` 0.16.5, `@colyseus/monitor` 0.16.7. Dev client `colyseus.js` 0.16.22. The Unity SDK must be a 0.16.x release. Express 4, zod 3, TypeScript 5.9.

## Additions beyond the contract

Nothing in section 7 was renamed. Extra helpers other phases may use:

- `core/errors/parse_input.ts`: `parse_input(schema, data)` throws `VALIDATION_FAILED`.
- `shared/utils/async_handler.ts`: express 4 does not catch async errors.
- `shared/constants/room_names.ts`: `ROOM_NAMES` (`lobby_room`, `game_room`, `presence_room`).
- `shared/constants/game_status.ts`: `GAME_STATUS` for `BaseGameState.status`.
- `shared/utils/token_utils.ts` also exports `payload_to_identity` and `extract_bearer_token`.
- `BaseRoom.authorize_identity(identity, options)`: protected hook. The game room overrides it to check the allowed player list, so it does not need to override `onAuth`.
- `module_registry.attach_room_features(room)` and `module_registry.get_room_features(room_name)`.
- `event_bus.on` returns an unsubscribe function. Handler errors are logged and never reach the emitter.

## Decisions to review

- `app.ts` registers a request logger (order 10) and `express.json` with a 10kb limit (order 15) so every module can read `req.body` on its own branch. Hardening also lists a 10kb JSON limit. If it adds its own parser it is a harmless second pass, since the body is already parsed.
- `noImplicitOverride` is off so room subclasses do not need `override` on every colyseus hook.
- `ServerError` from `onAuth` carries the error code as its message.
- `GET /health` returning 503 when the database is down is a small addition to the plan.
- `package.json` has no `"type": "module"`. The build is CommonJS, and the loaders use `require`.

## Verified

- `npm run typecheck` and `npm run build` pass.
- `npm run migrate` applies `0001_users.sql` and is safe to run twice. `users` has the columns from section 7.9.
- Server starts from `dist` and from `npm run dev`. `/health` returns `ok` with the database up.
- Loader picks up a module folder containing only an `index.ts` with a name, and a full module with routes, middleware, rooms, room features, jobs, `on_start` and `on_stop` (throwaway modules, removed afterwards).
- Placeholder game is loaded by the game loader.
- `require_auth` and `require_registered`: missing token gives `UNAUTHENTICATED`, bad token `INVALID_TOKEN`, guest on a registered route `FORBIDDEN`.
- Error shape: unknown route, thrown error (no internals), invalid json and oversized body all return the shared shape.
- `BaseRoom` with `colyseus.js`: join without token and with a bad token are rejected with the error code, a valid token joins, `get_identity` and `send_error` work, a `room_features` entry attaches without editing the room.
- Graceful shutdown on SIGTERM runs `on_stop` and exits cleanly.
- Production mode refuses default secrets. Invalid env values exit with a clear message.
- Token expiry: `TOKEN_EXPIRED` for an expired token, `INVALID_TOKEN` for a wrong secret, guest and user tokens use `JWT_GUEST_TTL_SECONDS` and `JWT_ACCESS_TTL_SECONDS`.
- `create_rate_limiter` blocks after the limit and allows again after the window. `generate_code` only uses the shared alphabet.

## Not verified

- `docker compose up -d`. The sandbox had no Docker daemon. Tests ran against a local PostgreSQL 16 with the same user, password and database name as the compose file.
- Windows paths. The migration runner and loaders use `path`, but only Linux was run.
