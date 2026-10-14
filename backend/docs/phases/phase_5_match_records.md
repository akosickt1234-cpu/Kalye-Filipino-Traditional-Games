# Phase 5: match records

Branch: `backend_match_records`. Base: `develop`. Plan version: v0.6.

## What is built

- `modules/match_records/` with migration `0020_matches.sql` (tables `matches` and `match_players`).
- A subscriber on `MATCH_FINISHED` that saves the match and its players in one transaction. The same `match_id` is ignored, so a repeated event is safe. Guest rows are saved with `user_id` null. There is no foreign key from `user_id` to `users`.
- Three routes (below), backed by plain SQL in `match_repository.ts`.
- The subscriber starts in `on_start` and is removed in `on_stop`.

## Routes added

- `GET /me/matches`: registered users only. Query `limit` (1 to 50, default 20) and `cursor`. Newest first. Returns `{ matches, next_cursor }`. Each match has `match_id`, `game_type`, `lobby_code`, `started_at`, `ended_at`, the caller's `team`, `result` and `score`, and all `players`. `next_cursor` is `null` on the last page. A bad cursor gives `VALIDATION_FAILED`.
- `GET /me/stats`: registered users only. Returns `{ total, games }`, where each entry has `played`, `wins`, `losses`, `draws` (and `game_type` for `games`).
- `GET /leaderboard/:game_type?limit=`: any signed in player, guests included. Limit 1 to 100, default 20. Returns `{ game_type, entries }` with `rank`, `user_id`, `username`, `wins`, `played`. Registered users only, ordered by wins, then fewer games played, then username. An unknown `game_type` returns an empty list, not an error.

## Env vars used

None beyond the shared ones (`DATABASE_URL` through the database client).

## Verified

Run with a throwaway script in `backend/tmp/` (git ignored) against a local PostgreSQL 16, with the module loaded the same way `server.ts` loads it:

- `npm run typecheck`, `npm run build` and `npm run migrate` pass.
- Four fake `MATCH_FINISHED` events plus one repeat create exactly four matches and the right player rows. Guest rows have null `user_id`.
- `/me/matches` paging with `limit` and `cursor`, newest first, last page has no cursor, bad cursor and limit above 50 rejected.
- `/me/stats` counts per game type and in total.
- `/leaderboard/placeholder_game` ranks by wins and excludes guests.
- Guests get `FORBIDDEN` on `/me/matches` and `/me/stats`. No token gives `UNAUTHENTICATED`.
- The subscriber is removed on `on_stop`.

## Not verified

- The full flow from the lobby module (`POST /lobbies/:code/result` to saved rows). The lobby phase is on another branch. This phase was tested by emitting the event directly.
- Behavior with Docker's `docker-compose.yml` database. A local PostgreSQL 16 install was used instead.
- Load or large data sets.

## Notes for integration

- `/leaderboard/*` uses `require_auth` on purpose, so guests can see the leaderboard. Keep it.
- The leaderboard joins `users`, so a deleted user drops off it. Their match rows stay.
- No requests for other phases were needed, so there is no file in `docs/requests/`.
