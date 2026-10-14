# Requests from phase 3: lobby

## Add a `GAME_CANCELLED` event

Problem: when the host cancels a started game, the lobby goes back to `waiting` but no event is emitted. The friends module builds presence from `GAME_STARTED` and `MATCH_FINISHED`, so those players would stay `in_game` after a cancel.

Change needed in foundation files:

- `shared/constants/event_names.ts`: add `GAME_CANCELLED: "game_cancelled"`.
- `shared/types/event_types.ts`: add `[EVENT_NAMES.GAME_CANCELLED]: { lobby_code: string; player_ids: string[] }`.

Then:

- lobby: emit it in `update_lobby` when `status: "waiting"` cancels a game. This is a one line change in `modules/lobby/lobby_service.ts`.
- friends: treat `GAME_CANCELLED` like `MATCH_FINISHED` for the membership index (players go back to `in_lobby`).

Until this is added, the lobby does not emit anything on cancel.
