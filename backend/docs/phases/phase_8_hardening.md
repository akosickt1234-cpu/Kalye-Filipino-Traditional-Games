# Phase 8: hardening

Branch: `backend_security`. Base: `develop`. Plan version: v0.6.

## What is built

`modules/hardening/`, loaded by the module loader. No shared file was edited.

| File | Purpose | Middleware order |
|---|---|---|
| `request_id.ts` | Sets `X-Request-Id` on every response and `res.locals.request_id`. A valid incoming id (8 to 64 chars, letters, digits, `_`, `-`) is kept, otherwise a uuid is made. | 20 |
| `security_headers.ts` | `helmet` with default headers. | 22 |
| `cors_policy.ts` | `cors` limited to `CORS_ORIGINS`. Requests without an `Origin` header (Unity clients) pass. Allowed methods: GET, POST, PATCH, DELETE, OPTIONS. Allowed headers: Authorization, Content-Type. | 24 |
| `rate_limit.ts` | `express-rate-limit` using `RATE_LIMIT_WINDOW_SECONDS` and `RATE_LIMIT_MAX`. Key is `player:<id>` when the bearer token verifies, otherwise `ip:<address>`. A limited request gets HTTP 429, `RATE_LIMITED`, the shared error shape, `Retry-After`, and `details.retry_after_seconds`. | 30 |
| `log_redaction_check.ts` | Run from `on_start`. Logs a probe with `authorization`, `password`, `token`, `refresh_token` (top level, nested, and in `req.headers`) into a capture stream. The server refuses to start if any value shows up. | on_start |

JSON body size: the 10 kb limit is already set by foundation at order 15 (`app.ts`), before any hardening middleware can run. It was not duplicated. Oversize bodies return HTTP 413 with `VALIDATION_FAILED`.

## Routes added

None.

## Env vars used

`CORS_ORIGINS`, `RATE_LIMIT_WINDOW_SECONDS`, `RATE_LIMIT_MAX`, `LOG_LEVEL` (through the logger). No new vars.

## Security review

- Raw errors: the error middleware maps `AppError`, zod errors, body parser errors, and everything else to the shared shape. An unexpected `Error` returns `INTERNAL_ERROR` with the fixed message and empty `details`. The stack and message go only to the server log.
- `npm audit` (all dependencies and production only): 0 vulnerabilities on 2026-10-14.
- Log redaction: passes at startup. `request_logger` logs only method, path, status, and duration, so bodies and headers are not logged by it.
- `x-powered-by` is disabled by foundation. Helmet adds the standard headers.

Findings:

1. The request id is not in the request log line yet. `request_logger` belongs to foundation (see `docs/requests/phase_8_hardening.md`).
2. Rate limit counters are in memory and reset on restart. Fine for local use.
3. Behind a tunnel (cloudflared, ngrok) or any proxy, `req.ip` is the proxy address unless `trust proxy` is set, so unauthenticated requests would share one limit. Needs `app.set("trust proxy", ...)` in `app.ts` (see requests file). Do not set it blindly, because a wrong value lets clients fake their IP.
4. A guest can ask `/auth/guest` for new tokens, and each token is a new player id with a new limit. The stricter `/auth/*` limit from phase 2 (by IP) is the control for that. The integration agent should confirm it is active.
5. Held long poll requests count once, when they arrive, as the plan says. A client in a lobby uses about 2 requests per 25 seconds plus presence, far below the default limit of 120 per minute.
6. Match results are reported by the host and cannot be verified. This is the open item in plan section 12, not a hardening gap.

## What was verified

With a throwaway script in `backend/tmp/` (git ignored), using the real `create_app` with the hardening module and a probe module, no database:

- `X-Request-Id` is present, a valid incoming id is kept, an invalid one is replaced.
- Helmet headers are present (`x-content-type-options`, `strict-transport-security`, `content-security-policy`).
- An allowed origin gets `access-control-allow-origin`, a disallowed origin gets none, a preflight returns 204 with the allowed methods.
- A route that throws an error with secret text returns HTTP 500, `INTERNAL_ERROR`, fixed message, no internals.
- A body over 10 kb returns 413 with the shared error shape.
- With `RATE_LIMIT_MAX=5`: player A got 5 responses of 200 and then 429. Player B from the same IP still got 200. The 429 body uses `RATE_LIMITED` and the response has `Retry-After`. A garbage bearer token falls back to the IP key.
- The redaction check passes, also with `LOG_LEVEL=silent`.
- `npm run typecheck` and `npm run build` pass. The real server boots with the hardening module loaded.
- `npm audit` shows 0 vulnerabilities.

## What could not be verified

- Boot with PostgreSQL up. Docker is not available here. The server boots with the database down and `/health` would report `down`.
- Interaction with the other phases' routes (auth rate limit on `/auth/*`, lobby and chat long polling under the global limit). Those branches are not merged into this one.
- Behavior through a tunnel or a reverse proxy.
- That the redaction check fails on a broken logger. It only passed on the current logger.
