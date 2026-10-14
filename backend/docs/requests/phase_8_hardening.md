# Requests from phase 8: hardening

## core/middleware/request_logger.ts (foundation)

Add the request id to the request log line so a client error report can be matched to a log entry. Hardening sets `res.locals.request_id` and the `X-Request-Id` header at order 20. The logger's `finish` handler runs later, so it can read it:

```
log.info({ request_id: res.locals.request_id, method: req.method, path: req.path, status: res.statusCode, duration_ms: Math.round(duration_ms) }, "request")
```

The same field can be added to the `unhandled request error` log in `core/middleware/error_middleware.ts`.

## app.ts (foundation)

Decide on `trust proxy` before testing through cloudflared or ngrok. Without it, unauthenticated requests share one rate limit key (the proxy address). Set it only to the real number of proxy hops, for example `app.set("trust proxy", 1)`, and only when a proxy is in front.
