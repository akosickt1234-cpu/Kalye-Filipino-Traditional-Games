# Request from phase 6 (friends)

Owner: lobby (`modules/lobby/`).

When the host cancels a game (`PATCH /lobbies/:code` with `status: "waiting"` while `in_game`), no event is emitted. The friends module only learns about `in_game` from `GAME_STARTED` and clears it on `MATCH_FINISHED`, `LOBBY_PLAYER_LEFT` (last member) or `LOBBY_CLOSED`. After a cancel, friends keep showing `in_game` instead of `in_lobby`.

Options for the integration agent:

- Add a new event in `shared/constants/event_names.ts` and `shared/types/event_types.ts`, for example `GAME_CANCELLED` with `lobby_code`, emitted by lobby on cancel. Friends would then clear its `in_game` flag for that code in `presence_service.ts`.
- Or accept the gap for casual play.

Friends does not change shared files, so nothing is blocked.
