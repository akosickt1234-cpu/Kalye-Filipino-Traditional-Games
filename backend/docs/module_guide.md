# Module guide

How to add a module and how to add a game. The loaders find both by scanning folders, so `app.ts` and `server.ts` are never edited.

## Add a module

1. Create `src/modules/<module_name>/index.ts` (snake_case folder name).
2. Default export a `ModuleDefinition` (see `src/shared/types/module_types.ts`).

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

Every field except `name` is optional:

| Field | Use |
|---|---|
| `routes(app)` | register express routes under your own prefix (section 7.10 of the plan) |
| `middleware` | global middleware, sorted by `order` (10 to 19 foundation, 20 to 39 hardening, 40 and up other modules) |
| `jobs` | interval jobs, `{ name, interval_ms, run }`. Runs never overlap, errors are logged |
| `on_start` | runs before the server listens |
| `on_stop` | runs on graceful shutdown, in reverse module order |

Rules:

- Modules talk to each other only through `event_bus` (`src/core/events/event_bus.ts`). Never import another module.
- Protect routes with `require_auth` or `require_registered` from `core/middleware/authentication_middleware.ts`. They read the token themselves, so they do not depend on the authentication module.
- Throw `AppError(code)` for expected errors. The error middleware turns it into `{ "error": { "code", "message", "details" } }`. Anything else becomes `INTERNAL_ERROR` with no details.
- Express 4 does not catch rejected promises. Wrap async handlers with `async_handler` from `shared/utils/async_handler.ts`.
- Validate input with zod and `parse_input(schema, data)` from `core/errors/parse_input.ts`.
- Read settings from `src/config/*`, never from `process.env`.
- Use `child_logger("<name>")` from `core/logging/logger.ts`. It redacts `authorization`, `password`, `token`, `refresh_token` and similar keys.
- Database access: `query` and `with_transaction` from `infrastructure/database/database_client.ts`.

### Migrations

Put plain `.sql` files in `src/modules/<module_name>/migrations/`. The runner collects `infrastructure/database/migrations/*.sql` and `modules/*/migrations/*.sql`, sorts all by filename, and applies each once (recorded in `schema_migrations`). Use only the number range reserved for your phase. Run with `npm run migrate`.

### Long polling

Lobby heartbeat and lobby chat use long polling (section 7.5 of the plan). The backend is REST only. Rules for every held request:

- Keep held requests in memory per lobby. Wake them when the lobby changes or a message arrives. Remove them when the client disconnects or `wait` ends.
- The client sends a version (`since` for the lobby, `after` for chat). If the current version is already newer, answer at once. Otherwise hold the request until the version changes or `wait` ends, then answer.
- `wait` is capped at `application_config.long_poll_max_seconds` (`LONG_POLL_MAX_SECONDS`). Larger values are clamped.
- A held request counts as a heartbeat. A member with an open held request is never timed out.
- A held request counts once for the global rate limit, when it arrives.
- Clients call again as soon as a request answers.

## Add a game

1. Create `src/games/<game_id>/index.ts`. The folder name must equal the game `id`.
2. Default export a `GameModule` (see `src/shared/types/game_types.ts`): `{ id, config }`.
3. Keep settings in `game_config.ts`, exporting a `GameConfig`: `display_name`, `min_players`, `max_players`, `team_count`.

A game is config only. The rules run in Unity, so the backend only uses the game list and its limits. The loader checks that `id` equals the folder name and that `min_players <= max_players`.

`games/placeholder_game/` is the reference.

`game_registry.get(game_id)` and `game_registry.list()` come from `core/loaders/game_loader.ts`. The lobby uses them, so a new game folder is available after a restart.

## Scripts

`npm run dev`, `npm run build`, `npm run typecheck`, `npm run migrate`, `npm start`.
