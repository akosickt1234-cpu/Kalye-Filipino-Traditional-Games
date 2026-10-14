# Kalye Backend

REST service for Kalye Filipino Traditional Games: accounts, lobbies, friends, chat and match records.

## About

Kalye is a multiplayer Unity game of traditional Filipino street games. This backend does not run matches. In-match play (positions, events, in-match chat) runs on Unity Netcode for GameObjects. The backend handles everything around the match:

- Accounts: email accounts and guests. A guest must enter a name.
- Lobbies: a public list and private join by code. The host publishes its connection info as `join_data`.
- Lobby chat: plain text, kept in memory.
- Friends: friend list, requests, lobby invites and presence.
- Match records and leaderboard: results are reported by the host client.
- Hardening: security headers, CORS, body limit, rate limit, request id.

It is plain HTTP and JSON. Lobby updates and lobby chat use long polling, so updates arrive in milliseconds on a local network. Lobbies, chat and presence live in memory and are lost on restart. Accounts, friends and match records live in PostgreSQL.

Stack: Node.js 22 or newer, TypeScript, Express 4, PostgreSQL, zod, jsonwebtoken, argon2, pino, helmet, cors, express-rate-limit.

## Installation

Needs Node.js, Docker and git.

```
cd backend
cp .env.example .env
docker compose up -d
npm install
npm run migrate
npm run dev
```

The server listens on `http://localhost:2567`.

- Same machine as Unity: use `localhost`.
- Another device on the same wifi: use the host machine's LAN IP and allow port 2567 in the firewall.
- From outside the network: use a tunnel such as cloudflared or ngrok, and set `TRUST_PROXY_HOPS=1` (see settings).
- Android blocks plain `http` by default. This is a Unity side setting.

Scripts: `npm run dev`, `npm run build`, `npm run typecheck`, `npm run migrate`, `npm start`.

### Settings

All settings are env vars, validated at startup in `src/config/environment.ts`. Code reads them through `src/config/*`, never from `process.env`.

| Var | Default |
|---|---|
| `NODE_ENV` | `development` |
| `PORT` | `2567` |
| `LOG_LEVEL` | `info` |
| `DATABASE_URL` | `postgres://kftg:kftg@localhost:5432/kftg` |
| `JWT_ACCESS_SECRET` | `change_me_change_me_change_me` |
| `JWT_ACCESS_TTL_SECONDS` | `900` |
| `JWT_GUEST_TTL_SECONDS` | `43200` |
| `REFRESH_TOKEN_TTL_DAYS` | `30` |
| `CORS_ORIGINS` | `http://localhost:3000` |
| `RATE_LIMIT_WINDOW_SECONDS` | `60` |
| `RATE_LIMIT_MAX` | `120` |
| `AUTH_RATE_LIMIT_MAX` | `10` |
| `LOBBY_CODE_LENGTH` | `5` |
| `LOBBY_HEARTBEAT_TIMEOUT_SECONDS` | `30` |
| `DEFAULT_GAME_TYPE` | `placeholder_game` |
| `PRESENCE_TIMEOUT_SECONDS` | `60` |
| `LONG_POLL_MAX_SECONDS` | `25` |
| `TRUST_PROXY_HOPS` | `0` |
| `FRIENDS_MAX` | `100` |
| `INVITE_TTL_SECONDS` | `300` |
| `CHAT_MAX_LENGTH` | `200` |
| `CHAT_RATE_MESSAGES` | `5` |
| `CHAT_RATE_WINDOW_SECONDS` | `10` |
| `CHAT_HISTORY_SIZE` | `30` |

- In production the server refuses to start while `JWT_ACCESS_SECRET` is still the default.
- `TRUST_PROXY_HOPS` is the number of proxies in front of the server. Keep it `0` with no proxy. With cloudflared or ngrok set it to `1`. If it is wrong, requests without a token share one rate limit key (the proxy address) or the client IP can be faked.

## Usage

### Requests and errors

- Send the access token as `Authorization: Bearer <token>`.
- Every response has an `X-Request-Id` header. Send it when reporting a problem so it can be matched to a log line.
- Errors always look like `{ "error": { "code": "...", "message": "...", "details": {} } }`. Stack traces and raw database errors are never sent.
- Codes: `VALIDATION_FAILED`, `UNAUTHENTICATED`, `INVALID_TOKEN`, `TOKEN_EXPIRED`, `FORBIDDEN`, `NOT_FOUND`, `CONFLICT`, `RATE_LIMITED`, `INTERNAL_ERROR`, `INVALID_CREDENTIALS`, `USERNAME_TAKEN`, `EMAIL_TAKEN`, `GUEST_NAME_INVALID`, `LOBBY_NOT_FOUND`, `LOBBY_FULL`, `LOBBY_IN_GAME`, `NOT_HOST`, `NOT_ENOUGH_PLAYERS`, `GAME_NOT_FOUND`, `ALREADY_FRIENDS`, `REQUEST_PENDING`, `INVITE_EXPIRED`, `CHAT_RATE_LIMITED`, `CHAT_TOO_LONG`.

### Routes

| Area | Routes | Who |
|---|---|---|
| Health | `GET /health` | none |
| Accounts | `POST /auth/register`, `/auth/login`, `/auth/guest`, `/auth/refresh`, `/auth/logout` | none |
| Profile | `GET /me`, `PATCH /me` | any, `PATCH` registered only |
| Lobbies | `POST /lobbies`, `GET /lobbies/public`, `GET /lobbies/:code`, `POST /lobbies/:code/join`, `/leave`, `/heartbeat`, `/kick`, `/result`, `PATCH /lobbies/:code` | any, some host only |
| Lobby chat | `POST /chat/:code`, `GET /chat/:code` | lobby members |
| Matches | `GET /me/matches`, `GET /me/stats`, `GET /leaderboard/:game_type` | registered (leaderboard any) |
| Friends | `GET /friends`, `/friends/requests`, `/friends/search`, `POST /friends/request`, `/accept`, `/decline`, `DELETE /friends/:user_id` | registered |
| Invites | `POST /invites`, `GET /invites`, `POST /invites/:id/decline` | registered |
| Presence | `POST /presence/heartbeat` | registered |

Notes on accounts:

- Login body uses `identifier` (email or username) and `password`.
- Guests get an id `guest_<uuid>`, a token valid for `JWT_GUEST_TTL_SECONDS`, no refresh token and no database row. A guest name is 3 to 20 characters, trimmed, with no double spaces, and cannot match a registered username.
- `PATCH /me` returns a fresh `access_token`, because the old one still holds the old username and avatar.
- Refresh tokens are random strings. Only the hash is stored. Each refresh rotates the token.

### Lobby rules for the Unity client

While in a lobby the client keeps two requests looping and sends each again as soon as it answers:

- `POST /lobbies/:code/heartbeat?since=<version>&wait=25` for lobby changes.
- `GET /chat/:code?after=<id>&wait=25` for chat.

Also call `POST /presence/heartbeat` about every 30 seconds. Set the client request timeout above 30 seconds. Without these loops, members and presence time out.

- A lobby view has `version`, which goes up on every change. A heartbeat answers at once when the lobby `version` is greater than `since` or `since` is missing. Otherwise the server holds it until something changes or `wait` ends. `wait` is clamped to `LONG_POLL_MAX_SECONDS`.
- A held request counts as a heartbeat and counts once for the global rate limit, when it arrives.
- `join_data` is an opaque string up to 256 characters (for example a Unity Relay join code). It is required on create but may be empty and set later with `PATCH`. Members get it in every lobby response. The public list and `GET /lobbies/:code` never include it.
- A player is in one lobby at a time. Creating or joining leaves the previous lobby. Join is rejected when full, in game, or when the player was kicked from that lobby. A repeated display name gets ` (2)` added.
- When the host leaves, the longest present member becomes host. An empty lobby closes at once. A member with no heartbeat for `LOBBY_HEARTBEAT_TIMEOUT_SECONDS` is removed.
- `PATCH` with `status: "in_game"` needs at least the game's `min_players` and saves a snapshot of player ids. `status: "waiting"` cancels. Other fields change only while `waiting`.
- `POST /lobbies/:code/result` is host only and only while `in_game`. Every `player_id` must be in the snapshot. A repeated `match_id` is accepted and saved once. The lobby goes back to `waiting`.
- A removed player gets `FORBIDDEN` on the next heartbeat. A closed lobby gives `LOBBY_NOT_FOUND`.
- Kicks are also enforced by Netcode in Unity. The backend only removes the player from the lobby list.

Chat: text is trimmed and stripped of control characters, empty text is rejected, and text over `CHAT_MAX_LENGTH` gives `CHAT_TOO_LONG`. The rate limit is `CHAT_RATE_MESSAGES` per `CHAT_RATE_WINDOW_SECONDS` per player. The last `CHAT_HISTORY_SIZE` messages are kept per lobby and cleared when the lobby closes. A player who left cannot read or send.

Friends: presence is `offline`, `online`, `in_lobby` or `in_game`. A player is `offline` when the last presence heartbeat is older than `PRESENCE_TIMEOUT_SECONDS`. Invites last `INVITE_TTL_SECONDS` and the sender must be friends with the target and be in that lobby. Clients find invites by polling `GET /invites`.

Rate limit: one global limit per `RATE_LIMIT_WINDOW_SECONDS`, keyed by player id when the request has a valid token and by IP otherwise. `/auth/*` has a stricter limit by IP using `AUTH_RATE_LIMIT_MAX`.

### Project layout

```
backend/
  src/
    server.ts, app.ts
    config/            one typed slice per area
    core/              errors, logging, middleware, lifecycle, loaders, events
    shared/            constants, types, utils
    infrastructure/    database client, migration runner, migrations
    modules/           authentication, lobby, match_records, friends, chat, hardening
    games/             placeholder_game
```

### Add a module

1. Create `src/modules/<module_name>/index.ts` (snake_case folder).
2. Default export a `ModuleDefinition` from `src/shared/types/module_types.ts`. Only `name` is required.

```ts
import type { ModuleDefinition } from "../../shared/types/module_types"

const definition: ModuleDefinition = {
  name: "example",
  routes(app) {
    app.get("/example", (_req, res) => res.json({ ok: true }))
  }
}

export default definition
```

| Field | Use |
|---|---|
| `routes(app)` | register routes under your own prefix |
| `middleware` | global middleware sorted by `order` (10 to 19 foundation, 20 to 39 hardening, 40 and up other modules) |
| `jobs` | interval jobs `{ name, interval_ms, run }`. Runs never overlap, errors are logged |
| `on_start` | runs before the server listens |
| `on_stop` | runs on graceful shutdown, in reverse module order. The http server is already closing, so release held requests here |

The loader scans `modules/*/index.ts`, so `app.ts` is never edited. Rules:

- Modules talk to each other only through `event_bus` (`src/core/events/event_bus.ts`). Never import another module.
- Protect routes with `require_auth` or `require_registered` from `core/middleware/authentication_middleware.ts`. They check the token themselves.
- Throw `AppError(code)` for expected errors. Anything else becomes `INTERNAL_ERROR` with no details.
- Express 4 does not catch rejected promises. Wrap async handlers with `async_handler`.
- Validate input with zod and `parse_input(schema, data)`.
- Log with `child_logger("<name>")`. It redacts `authorization`, `password`, `token`, `refresh_token` and similar keys, and adds the request id to request logs.
- Database access: `query` and `with_transaction` from `infrastructure/database/database_client.ts`.
- Migrations are plain `.sql` files in `src/modules/<module_name>/migrations/`. The runner sorts all migrations by filename and applies each once. Reserved ranges: foundation 0001 to 0009, authentication 0010 to 0019, match_records 0020 to 0029, friends 0030 to 0039.
- Long polling: keep held requests in memory per lobby, wake them on change, remove them on disconnect or when `wait` ends, and release them in `on_stop`.

Events (`shared/constants/event_names.ts`): `LOBBY_OPENED`, `LOBBY_CLOSED`, `LOBBY_PLAYER_JOINED`, `LOBBY_PLAYER_LEFT`, `GAME_STARTED`, `GAME_CANCELLED`, `MATCH_FINISHED`. The lobby emits all of them. `LOBBY_PLAYER_LEFT` is also sent on kick and timeout.

### Add a game

1. Create `src/games/<game_id>/index.ts`. The folder name must equal the game `id`.
2. Default export a `GameModule`: `{ id, config }`.
3. Keep settings in `game_config.ts`: `display_name`, `min_players`, `max_players`, `team_count`.

A game is config only, because the rules run in Unity. `games/placeholder_game/` is the reference. The lobby reads games through `game_registry.get` and `game_registry.list`, so a new folder is available after a restart.

### Known limits

- Match results come from the host client and cannot be checked. A modified host can report a fake winner or score. Fine for casual play. Later options: all players confirm the result, or a dedicated server runs the game.
- The real game list and rules are not decided. Everything uses the placeholder game (2 to 6 players, 1 team).
- Friends presence and invites are checked about every 30 seconds, so they can be that late.
- A saved match reaches `/me/matches` and `/me/stats` through the event bus, so it can show up a few milliseconds after the result request answers.

## Credits

Author: arkeos-kzenon

Published: 2026-10-14

## Version history

- v0.1: foundation, loaders, event bus, database and migrations, health route, placeholder game
- v0.2: authentication, lobby, match records, chat, friends and hardening modules merged. Added `GAME_CANCELLED` emit, request id in logs, and `TRUST_PROXY_HOPS`
