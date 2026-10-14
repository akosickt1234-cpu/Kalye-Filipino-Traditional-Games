# Phase 7: chat

Branch: `backend_chat`. Base: `develop`. Plan version: v0.6.

## What is built

- `modules/chat/` with in-memory lobby chat over REST and long polling. No database, no migrations, no sockets.
- Membership is built only from `LOBBY_OPENED`, `LOBBY_PLAYER_JOINED`, `LOBBY_PLAYER_LEFT` and `LOBBY_CLOSED`. The lobby module is never imported.
- `chat_store.ts` keeps members, history (last `CHAT_HISTORY_SIZE`), the per lobby message id counter and the held requests.
- `text_filter.ts` has `filter_text(text)`. It returns the text unchanged and is the place for a word list later.
- The subscriber starts in `on_start` and is removed in `on_stop`, which also releases held requests and clears memory.

## Routes added

- `POST /chat/:code`: body `{ text }`. Returns `201` with `{ id, from_id, username, text, sent_at }`. The sender name is `req.player.username`.
- `GET /chat/:code?after=<id>&wait=<seconds>`: returns `{ messages }`. Without `after` it returns the history. With `after` it returns messages with a higher id. With `after` and `wait`, and nothing new, the request is held until a message arrives or `wait` ends. `wait` is clamped to `LONG_POLL_MAX_SECONDS`. `wait` without `after` is ignored and the history is returned at once.

Both routes need `require_auth`. Unknown code gives `LOBBY_NOT_FOUND`. A caller who is not a member gives `FORBIDDEN`. Codes are matched case insensitively.

## Rules applied on send

Order of checks: lobby exists, caller is a member, body is valid, text is cleaned (control characters removed, trimmed), empty text gives `VALIDATION_FAILED`, longer than `CHAT_MAX_LENGTH` characters gives `CHAT_TOO_LONG`, then the rate limit (`CHAT_RATE_MESSAGES` per `CHAT_RATE_WINDOW_SECONDS` per player id) gives `CHAT_RATE_LIMITED`. Rejected messages do not use up the rate limit. The limit counter of a player is reset when they leave a lobby.

## Behavior notes for other phases and Unity

- A held request is released with `FORBIDDEN` when the caller leaves, is kicked or times out, and with `LOBBY_NOT_FOUND` when the lobby closes. A client that disconnects leaves no held request behind.
- Message ids start at 1 per lobby and rise by one. Ids are not reused after the history is trimmed. History and ids are lost when the lobby closes or the server restarts.
- A held chat request does not refresh the lobby heartbeat time, because the shared event list has no event for it and modules cannot import each other. Clients keep the lobby heartbeat loop running as described in section 12 of the plan, and that loop keeps the member alive.
- Chat membership depends on the lobby events. If the lobby module is not loaded, every chat route gives `LOBBY_NOT_FOUND`.

## Env vars used

`CHAT_MAX_LENGTH`, `CHAT_RATE_MESSAGES`, `CHAT_RATE_WINDOW_SECONDS`, `CHAT_HISTORY_SIZE`, `LONG_POLL_MAX_SECONDS`.

## Verified

Run with a throwaway script in `backend/tmp/` (git ignored). It loads only the chat module into the real express app and drives it with fake lobby events:

- `npm run typecheck` and `npm run build` pass.
- send trims and strips control characters, ids rise, case insensitive code, history without `after`, `after` filter in order, no `wait` answers at once.
- empty text, text over the limit and a non string body are rejected with the right codes.
- a non member and a player who left cannot read or send. An unknown lobby gives `LOBBY_NOT_FOUND`. A bad token gives `UNAUTHENTICATED`.
- a held request answers within 150 ms of a new message, ends empty when `wait` runs out, and is released with `FORBIDDEN` when the player leaves and with `LOBBY_NOT_FOUND` when the lobby closes.
- a client disconnect leaves no held request. History is cleared after the lobby closes. History keeps only the last 30 messages.
- the rate limit allows 5 messages per window and then returns `CHAT_RATE_LIMITED`.
- `on_stop` removes all four event listeners.

## Not verified

- The full flow with the real lobby module (`LOBBY_*` events came from the test script, not from lobby routes). The lobby phase is on another branch.
- A server start with the database through `npm run dev` and docker.
- A Unity client, many concurrent players, or load.
- The global rate limit for held requests, which belongs to phase 8.
