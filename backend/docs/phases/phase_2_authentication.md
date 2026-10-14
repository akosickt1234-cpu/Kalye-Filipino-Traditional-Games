# Phase 2: authentication

Branch: `backend_authentication`. Base: `develop`. Plan version: v0.6.

## What is built

Module `src/modules/authentication/` and migration `0010_refresh_tokens.sql`. No shared file was changed.

- `authentication_routes.ts`: all routes below.
- `authentication_service.ts`: register, login, guest, refresh rotation, logout, `/me` read and update.
- `authentication_schemas.ts`: zod schemas for every request body.
- `guest_name.ts`: guest name rules.
- `refresh_token_store.ts`: random opaque refresh tokens, only the sha256 hash is stored.
- `authentication_rate_limit.ts`: stricter limit on `/auth/*`, keyed by IP, using `AUTH_RATE_LIMIT_MAX` per `RATE_LIMIT_WINDOW_SECONDS`. Registered as a module middleware entry at order 40.
- `index.ts`: module definition plus an hourly job that deletes expired refresh tokens.

## Routes

| Route | Auth | Body | Result |
|---|---|---|---|
| `POST /auth/register` | none | `username`, `email`, `password` | `201` `{ access_token, refresh_token, user }` |
| `POST /auth/login` | none | `identifier` (email or username), `password` | `{ access_token, refresh_token, user }` |
| `POST /auth/guest` | none | `username` (required), `avatar?` | `201` `{ access_token, user }` |
| `POST /auth/refresh` | none | `refresh_token` | new `{ access_token, refresh_token, user }`, old token revoked |
| `POST /auth/logout` | none | `refresh_token` | `204`, also `204` for an unknown token |
| `GET /me` | any | none | `{ user }` |
| `PATCH /me` | registered | `username?`, `avatar?` (at least one) | `{ access_token, user }` |

Notes for the Unity team:

- The login body field is `identifier`, because the plan did not name it.
- `PATCH /me` returns a fresh `access_token` because the old one still carries the old username and avatar until it expires.
- `/me` for a registered user reads the database. For a guest it reads the token.
- Only registered users get `email` in the `user` object.
- A guest can send an optional `avatar` from `AVATAR_IDS`. Default is `boy1`.

## Error codes used

`VALIDATION_FAILED`, `INVALID_CREDENTIALS`, `USERNAME_TAKEN`, `EMAIL_TAKEN`, `GUEST_NAME_INVALID`, `INVALID_TOKEN` (bad, expired or revoked refresh token), `UNAUTHENTICATED`, `FORBIDDEN` (guest on `PATCH /me`), `NOT_FOUND` (account deleted), `RATE_LIMITED`.

A guest name that matches a registered username gives `USERNAME_TAKEN`. A missing or rule breaking guest name gives `GUEST_NAME_INVALID`.

## Env vars used

`JWT_ACCESS_SECRET`, `JWT_ACCESS_TTL_SECONDS`, `JWT_GUEST_TTL_SECONDS`, `REFRESH_TOKEN_TTL_DAYS`, `AUTH_RATE_LIMIT_MAX`, `RATE_LIMIT_WINDOW_SECONDS`. All already defined by foundation.

## Verified

Against a temporary local PostgreSQL 16 with `npm run migrate` and `npm run dev`:

- `npm run typecheck` and `npm run build` pass.
- Register, duplicate username, duplicate email (case insensitive), bad input.
- Login by email and by username, wrong password and unknown user give the same `INVALID_CREDENTIALS`.
- Guest: missing, too short, double space, bad character, registered name, and a valid unicode name with extra spaces trimmed.
- A guest token passes `verify_access_token`, has id `guest_<uuid>`, 12 hour lifetime, and creates no database row.
- `/me` for registered, guest, and no token. `PATCH /me` for a guest gives `FORBIDDEN`. Avatar and username change work, bad avatar and empty body are rejected.
- Refresh rotates the token, the old token is rejected, logout revokes, refresh after logout is rejected. Only hashes are in the table.
- The cleanup job deletes expired rows.
- The `/auth/*` limit gives `RATE_LIMITED` after 10 requests in a window, and `/me` is not counted.
- The server log has no passwords or tokens in it.

## Not verified

- Behaviour together with the other phases (lobby, friends, hardening). The global rate limit from the hardening phase will also apply on top of the `/auth/*` limit.
- Concurrent refresh calls with the same token. The row is locked in a transaction, so one wins and the other gets `INVALID_TOKEN`.
- Behind a proxy. `req.ip` is used for the limit and `trust proxy` is not set.
- The production secret check (foundation owns it).
