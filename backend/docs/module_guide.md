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
| `rooms` | colyseus rooms to define, `{ name, room_class, options? }` |
| `jobs` | interval jobs, `{ name, interval_ms, run }`. Runs never overlap, errors are logged |
| `room_features` | `{ room_names, attach(room) }`, runs in `BaseRoom.onCreate` for matching rooms |
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

### Rooms

Extend `BaseRoom` from `core/rooms/base_room.ts` and call `super.onCreate(options)` first.

- `onAuth` reads `options.token`, verifies it and returns a `PlayerIdentity`. Read it with `this.get_identity(client)`.
- Auth failures throw a `ServerError` whose message is the error code (`UNAUTHENTICATED`, `INVALID_TOKEN`, `TOKEN_EXPIRED`).
- To add checks after the token is valid, override `authorize_identity(identity, options)` and throw to reject. Do not override `onAuth`.
- Send room errors with `this.send_error(client, code, message?)`.

## Add a game

1. Create `src/games/<game_id>/index.ts`. The folder name must equal the game `id`.
2. Default export a `GameModule` (see `src/shared/types/game_types.ts`).
3. Keep settings in one file, `game_config.ts`, exporting a `GameConfig`.
4. Keep rules in `game_logic.ts`. `index.ts` only wires them together.

`games/placeholder_game/` is the reference. `game_logic.ts` exports `create_state`, `on_player_input`, `on_player_action`, `on_tick`, `is_finished`, `get_results` and `on_player_leave`. `index.ts` imports those names, so keep them when replacing the logic.

State classes extend `BaseGameState` and `BaseGamePlayer` from `shared/schemas/base_game_state.ts`. Use `@type` decorators from `@colyseus/schema`.

The game module validates its own input shape. The game room drops anything that throws.

`game_registry.get(game_id)` and `game_registry.list()` come from `core/loaders/game_loader.ts`. The lobby and the game room use them, so a new game folder is available everywhere after a restart.

## Scripts

`npm run dev`, `npm run build`, `npm run typecheck`, `npm run migrate`, `npm start`.
