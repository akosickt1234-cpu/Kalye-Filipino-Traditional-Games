# Phase 6: friends, invites, presence

Branch: `backend_friends`. Base: `develop`. Plan version: v0.6.

## What is built

- `modules/friends/` with routes, services, schemas, types and two migrations.
- Migrations `0030_friendships.sql` and `0031_room_invites.sql`. Pair uniqueness on `friendships` is enforced in both directions with a unique index on `least` and `greatest` of the two user ids.
- Presence kept in memory: last heartbeat time per user, plus a lobby index built only from `LOBBY_PLAYER_JOINED`, `LOBBY_PLAYER_LEFT`, `GAME_STARTED`, `MATCH_FINISHED` and `LOBBY_CLOSED`. Guest ids are ignored.
- Status order: `offline` when the last heartbeat is older than `PRESENCE_TIMEOUT_SECONDS` or missing, then `in_game` or `in_lobby` from the index, otherwise `online`.
- Job `friends_delete_expired_invites` runs every 60 seconds. It deletes invites expired for more than one hour (the grace lets `POST /invites/:id/decline` answer `INVITE_EXPIRED`) and prunes old presence entries.

## Routes added

All routes use `require_registered`. Guests get `FORBIDDEN`.

| Route | Body or query | Result |
|---|---|---|
| `GET /friends` | none | `{ friends: [{ id, username, avatar, status }] }` |
| `GET /friends/requests` | none | `{ incoming, outgoing }`, each item `{ id, username, avatar, created_at }` |
| `GET /friends/search?q=` | `q`: 1 to 20 chars, letters, digits, underscore | `{ users: [{ id, username, avatar, relation }] }`, up to 10, prefix match. `relation` is `none`, `friends`, `request_sent` or `request_received` |
| `POST /friends/request` | `user_id` | `201 { user_id, status: "pending" }`, or `200 { user_id, status: "accepted" }` when the other user had already asked you |
| `POST /friends/accept` | `user_id` (the requester) | `204` |
| `POST /friends/decline` | `user_id` (the requester) | `204` |
| `DELETE /friends/:user_id` | none | `204`. Removes a friend or cancels your own pending request. Also deletes invites between the two users |
| `POST /invites` | `to_user_id`, `lobby_code` | `201 { id, to_user_id, lobby_code, expires_at }` |
| `GET /invites` | none | `{ invites: [{ id, from: { id, username, avatar }, lobby_code, created_at, expires_at }] }`, pending and not expired |
| `POST /invites/:id/decline` | none | `204` |
| `POST /presence/heartbeat` | none | `204` |

Errors used: `VALIDATION_FAILED`, `NOT_FOUND`, `ALREADY_FRIENDS`, `REQUEST_PENDING`, `FORBIDDEN`, `CONFLICT` (friend limit, target already in the lobby), `LOBBY_IN_GAME`, `INVITE_EXPIRED`.

## Behavior notes

- Sending a request to someone who already sent you one accepts it.
- An invite needs: sender and target are friends, the sender is in that lobby (from the event index), the lobby is not `in_game`, and the target is not already in it. Lobby codes are uppercased. Sending the same invite again refreshes the expiry instead of adding a row.
- When the invited user joins the lobby, their pending invite for it is marked `accepted`. When a lobby closes, its invites are deleted.
- `FRIENDS_MAX` is checked for both users when a friendship becomes accepted.

## Env vars used

`FRIENDS_MAX`, `INVITE_TTL_SECONDS`, `PRESENCE_TIMEOUT_SECONDS` (all through `config/friends_config.ts`).

## Verified

With PostgreSQL 16 and a throwaway script (`backend/tmp/check_friends.ts`, git ignored) that runs the app in one process and emits the lobby events itself:

- `npm run typecheck` and `npm run build` pass, migrations apply.
- Guest and missing token rejected. Search, request, duplicate request, self request, unknown user, accept, wrong side accept, already friends, remove, remove again.
- Presence goes `offline`, `online`, `in_lobby`, `in_game`, back to `in_lobby` after `MATCH_FINISHED`, and `online` after `LOBBY_CLOSED`.
- Invite rules (not a member, not a friend, target already inside), create, refresh, list, decline, decline again, bad id, accepted on join, expired invite removed by the job.

## Could not verify

- Presence going back to `offline` after the timeout (not waited for).
- Friend limit (`FRIENDS_MAX` not lowered in the check).
- Behavior with the real lobby module. The lobby phase was not merged here, so events were emitted by the script. The lobby branch emits `LOBBY_PLAYER_JOINED` for the host too, which this module relies on.
- Two simultaneous requests racing at the friend limit.

## Known gap

Cancelling a game with `PATCH status: "waiting"` emits no event in the contract, so a friend can show `in_game` until the match result, a leave, or the lobby closing. See `docs/requests/phase_6_friends.md`.
