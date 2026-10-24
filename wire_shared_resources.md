# wire shared resources

shared code for talking to the backend. folder: `Assets/Scripts/Api`. all calls go through `ApiClient`. never use `UnityWebRequest` directly in feature code.

## ApiConfig

| member | value |
| --- | --- |
| `BASE_URL` | `http://localhost:2567` (also used by `LoginService`) |
| `TIMEOUT_SECONDS` | 15 |
| `LONG_POLL_TIMEOUT_SECONDS` | 35 |

```csharp
var url = ApiConfig.BASE_URL + "/me";
```

## ApiClient

static class. the bearer token comes from `LoginSession`. callbacks run on the main thread. on `UNAUTHENTICATED`, `TOKEN_EXPIRED` or `INVALID_TOKEN` it tries one refresh through `LoginService.restore` and retries once. if the refresh is rejected, or the retry fails the same way, it signs out and opens the login overlay. if the refresh fails only because the network is down, the saved session is kept, the login overlay does not open, and the caller gets a normal network failure (`is_network_failure` true, `session_ended` false), so show the retry toast. guests have no refresh token, so they go straight to the login overlay. when a refresh is already running, other calls wait for it.

| member | use |
| --- | --- |
| `get<T>(path, on_ok, on_fail)` | GET, reply parsed into `T` |
| `get_long_poll<T>(path, on_ok, on_fail)` | GET with the 35 second timeout |
| `post<T>(path, body, on_ok, on_fail)` | POST, `body` is any `[Serializable]` object or null |
| `patch<T>(path, body, on_ok, on_fail)` | PATCH, same body rules |
| `delete(path, on_ok, on_fail)` | DELETE, no reply body expected (`on_ok` takes no argument) |

`T` must be a `[Serializable]` class. a reply that is empty or does not parse gives `ApiError.CODE_BAD_REPLY`. `path` must start with a single `/`.

```csharp
ApiClient.get<ApiModels.MeReply>("/me",
    reply => PlayProfile.apply(reply),
    error =>
    {
        if (!error.session_ended) ApiToast.toast_retry(error.message, retry);
    });

ApiClient.patch<ApiModels.MeReply>("/me", new AvatarBody { avatar = "boy1" }, on_ok, on_fail);
ApiClient.delete("/friends/abc", () => refresh(), on_fail);
```

## ApiError

reaches every `on_fail`. never show raw backend text. use `message`.

| member | meaning |
| --- | --- |
| `code` | backend error code, or `NETWORK_FAILURE`, `BAD_REPLY`, `UNKNOWN` |
| `status` | http status, 0 when there was none |
| `message` | friendly text, safe to show |
| `session_ended` | true only when the session was lost and the user was sent to the login overlay (rejected refresh, guest token expired, or a second auth failure after a retry). do not toast in that case. false for network failures, even during a refresh |
| `is_network_failure` | true for connection errors and timeouts |
| `ApiError.friendly(code)` | friendly text for a code. unknown codes get a generic text |

mapped codes: `VALIDATION_FAILED`, `UNAUTHENTICATED`, `INVALID_TOKEN`, `TOKEN_EXPIRED`, `FORBIDDEN`, `NOT_FOUND`, `CONFLICT`, `RATE_LIMITED`, network failure.

```csharp
if (error.is_network_failure) ApiToast.toast_retry(error.message, retry);
```

## ApiRunner

coroutine host that survives scene loads. `ApiClient` uses it. feature code rarely needs it.

```csharp
ApiRunner.run(my_routine());
```

## ApiModels

`[Serializable]` reply classes, nested in the static class `ApiModels`.

| class | fields |
| --- | --- |
| `MeReply` | `user` (`MeUser`) |
| `MeUser` | `id`, `username`, `is_guest`, `avatar` |
| `StatsReply` | `total` (`StatsCounts`), `games` (`StatsGame[]`) |
| `StatsCounts` | `played`, `wins`, `losses`, `draws` |
| `StatsGame` | `game_type`, `played`, `wins`, `losses`, `draws` |

```csharp
var played = reply.total != null ? reply.total.played : 0;
```

treat every value as untrusted. `avatar` is compared to a fixed list and never used as a path.

## ApiToast

own canvas (sort order 5000, kept across scenes), so it works in the Play scene. reuses the `LoginToast` look. the retry button needs an `EventSystem` in the scene, which the Play scene already has.

| member | use |
| --- | --- |
| `toast(message, is_error)` | short message, error or success color |
| `toast_retry(message, retry_action)` | error toast with a RETRY button, stays 10 seconds |

```csharp
ApiToast.toast("saved", false);
ApiToast.toast_retry("could not load your profile", () => PlayProfile.load(true));
```

## endpoints

both need a bearer token. error body shape for every failure: `{ "error": { "code": "...", "message": "...", "details": {} } }`.

### GET /me

works for guests and registered users.

```json
{ "user": { "id": "...", "username": "...", "is_guest": false, "avatar": "boy1", "email": "..." } }
```

`email` is only sent for registered users and is not part of `MeUser`. `avatar` is `boy1` (Timmy) or `girl1` (Amy), read from `authentication_service.ts` (`get_me`).

### GET /me/stats

registered only. guests get `FORBIDDEN`, so never call it for a guest (`LoginSession.is_guest`).

```json
{ "total": { "played": 0, "wins": 0, "losses": 0, "draws": 0 },
  "games": [ { "game_type": "...", "played": 0, "wins": 0, "losses": 0, "draws": 0 } ] }
```

total played is `total.played`. read from `match_routes.ts`.
