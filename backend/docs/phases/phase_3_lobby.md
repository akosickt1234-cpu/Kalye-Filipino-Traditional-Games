# Phase 3: lobby

Branch: `backend_lobby`. Owns `src/modules/lobby/`.

## What was built

An in memory lobby directory with long polling. No sockets, no database. Lobbies are lost on restart.

Files in `src/modules/lobby/`:

- `index.ts`: module definition, job `remove_stale_members` every 5 seconds, `on_stop` releases held requests.
- `lobby_routes.ts`: the routes below.
- `lobby_service.ts`: all lobby state and rules, event emits, held request handling.
- `lobby_schemas.ts`: zod schemas for bodies and query.
- `lobby_views.ts`: lobby view, public list item, summary.
- `lobby_types.ts`: types.

## Routes added

All use `require_auth` (guests allowed).

- `POST /lobbies`
- `GET /lobbies/public`
- `GET /lobbies/:code`
- `POST /lobbies/:code/join`
- `POST /lobbies/:code/leave`
- `POST /lobbies/:code/heartbeat?since=&wait=`
- `PATCH /lobbies/:code`
- `POST /lobbies/:code/kick`
- `POST /lobbies/:code/result`

`POST /presence/heartbeat` is not here. Section 7.9 and phase 6 give `/presence/*` to the friends module.

## Env vars used

`LOBBY_CODE_LENGTH`, `LOBBY_HEARTBEAT_TIMEOUT_SECONDS`, `DEFAULT_GAME_TYPE`, `LONG_POLL_MAX_SECONDS`.

## Behavior notes for other phases and Unity

- Events emitted: `LOBBY_OPENED`, then `LOBBY_PLAYER_JOINED` for the host on create. Chat and friends build membership from `LOBBY_PLAYER_JOINED`, and friends ignores `LOBBY_OPENED`, so the host gets a joined event too.
- Moving to another lobby emits `LOBBY_PLAYER_LEFT` for the old one first. Kick and timeout also emit `LOBBY_PLAYER_LEFT`. When the last member leaves, `LOBBY_PLAYER_LEFT` is emitted, then `LOBBY_CLOSED`.
- `GAME_STARTED` and `MATCH_FINISHED` are emitted after the lobby state is updated.
- Cancelling a game (`status: "waiting"` while `in_game`) emits no event. See `docs/requests/phase_3_lobby.md`.
- `join_data` is required on create but may be an empty string, because the host may not have its Netcode session ready yet. It can be set later with `PATCH`.
- Every member facing response (create, join, heartbeat, patch, kick, result) includes `join_data`, so clients see a changed value through the heartbeat. The public list and `GET /lobbies/:code` never include it.
- `POST /lobbies/:code/result` returns `201` with the lobby view. A repeated `match_id` returns `201` and emits nothing. A new result when the lobby is not `in_game` returns `CONFLICT`.
- `max_players` is clamped to the game's min and max. If the clamped value is below the current member count, the request fails with `CONFLICT`.
- Join order of checks: unknown code, already a member (returns the view), kicked (`FORBIDDEN`), not `waiting` (`LOBBY_IN_GAME`), full (`LOBBY_FULL`). A failed join does not remove the player from their current lobby.
- Codes are matched case insensitively.
- The host cannot kick themselves (`VALIDATION_FAILED`). Kicking a non member gives `NOT_FOUND`.
- A held heartbeat from the host or a member is never timed out by the job. When a held request ends for any reason, the member's heartbeat time is refreshed.
- A held request is released with `FORBIDDEN` if the caller was removed, or `LOBBY_NOT_FOUND` if the lobby closed.

## What was verified

Throwaway scripts in `backend/tmp/` (git ignored) ran the real express app in process with no database:

- create public and private, join by code, join from the public list, display name suffix, leave, full, kicked cannot rejoin, unknown game type, max players clamp
- heartbeat with and without `since`, held heartbeat answered in under 450 ms when a player joined, and in under 350 ms on kick, on start, and on host leave
- start with enough players, cancel, start again, result rejected for an unknown player, result host only, `MATCH_FINISHED` payload filled by the server, repeated `match_id` emits once
- host transfer to the longest member, empty lobby closes at once
- silent member removed by the job (with the timeout set to 1 second), member with a held request is kept, client disconnect leaves no held request behind
- `npm run typecheck` and `npm run build` pass

## What could not be verified

- No Unity client was used.
- The full server start with a database (`npm run dev` with docker) was not run. The lobby module does not use the database.
- Behavior with many concurrent players was not load tested.
- The global rate limit for held requests belongs to phase 8 and was not checked here.
