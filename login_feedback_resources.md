# login feedback resources

branch: feature_login_feedback (off feature_login_shared)

| file | what other agents use it for |
| --- | --- |
| Assets/Scripts/Login/LoginFeedback.cs | `build(root)` (already called by the overlay), `toast(message, is_error)`, `toast_retry(message, retry)`, `ask_guest_name(on_name)` |
| Assets/Scripts/Login/LoginToast.cs | toast view used by `LoginFeedback`, not called directly |
| Assets/Scripts/Login/LoginGuestModal.cs | guest name modal used by `LoginFeedback`, not called directly. disables `LoginOverlayHost` while open so escape closes only the modal |
| Assets/Scripts/Login/LoginService.cs | `login`, `guest`, `register`, `forgot`, `restore`, `sign_out()`, `sign_out(on_done)`. callbacks run on the main thread. on_fail always gets friendly text |
| Assets/Scripts/Login/LoginStore.cs | `save`, `load`, `clear`. refresh token only, encrypted file in `Application.persistentDataPath` |

## notes

- network failure: the service shows a retry toast itself and also calls `on_fail` with the same text. a plain `toast` with that same text is ignored while the retry toast is showing, so callers can keep showing `on_fail` text.
- guest body field is `username` (backend `guest_schema`), not `name`.
- forgot always calls `on_fail("this is not available yet")`.
- the store is weak on non-windows platforms: AES key comes from the device id.
- `Allow downloads over HTTP` must be enabled in player settings for the http base url.
