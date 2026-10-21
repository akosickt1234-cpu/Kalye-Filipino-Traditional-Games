# login shared resources

branch: feature_login_shared (off arkeos_merge)

## owned by this branch (shared skeleton)

| file | what other agents use it for |
| --- | --- |
| Assets/Scripts/Login/LoginOverlay.cs | `open(Action)`, `close()`, `require_login(Action)`, `succeed()` (close + run on_success, call after a good login), `root`, `is_open` |
| Assets/Scripts/Login/LoginState.cs | `LoginMode {SignIn, Register, Forgot}`, `mode`, `set_mode()`, `set_busy()`, `submit()`, `is_busy`, events `on_mode_changed`, `on_submit`, `on_busy_changed` |
| Assets/Scripts/Login/LoginSession.cs | memory only: `is_logged_in`, `is_guest`, `access_token`, `refresh_token`, `display_name`, `set(name, access, refresh, guest)`, `clear()` |
| Assets/Scripts/Login/LoginStore.cs | `save(refresh_token)`, `load()`, `clear()` (PlayerPrefs, see notes) |
| Assets/Scripts/Login/LoginService.cs | stubs, all fail with "Login is not available yet." except `sign_out()` which clears session and store |
| Assets/Scripts/Login/LoginTheme.cs | login only tokens: colors, layout fractions, font sizes, art paths |
| Assets/Scripts/Login/LoginUi.cs | `view(name, parent, modes)` creates a stretched child that toggles on mode change, `rounded_box()`, `icon()` |
| Assets/Scripts/Login/LoginModeToggle.cs | `attach(view, modes)`, `ALL_MODES` |
| Assets/Scripts/Login/LoginFocusGlow.cs | `attach(selectable, tint, normal, lit, glow, glow_color)` keyboard focus and hover tint, uses `PlayFocus.lit` |
| Assets/Scripts/Login/LoginField.cs, LoginFieldView.cs | `create(parent, placeholder, icon_path, is_password)` returns `LoginFieldView` with `root`, `get_value()`, `set_error()`, `set_enabled()`, `clear()`. enter inside a field calls `LoginState.submit()` |
| Assets/Scripts/Login/LoginFrame.cs | builds the card, returns the inner rect, `card` is the outer rect |
| Assets/Scripts/Login/LoginBackground.cs, LoginBlurCapture.cs | blurred screen behind the card, falls back to the modal blocker dim |
| Assets/Scripts/Login/LoginOverlayHost.cs | escape closes, tab and shift+tab cycle, focus is kept inside the overlay |
| Assets/Scripts/Login/LoginHeader.cs | stub, shown in all modes |
| Assets/Scripts/Login/LoginForm.cs, LoginActions.cs | stubs, shown in SignIn |
| Assets/Scripts/Login/LoginRegister.cs | stub, shown in Register |
| Assets/Scripts/Login/LoginForgot.cs | stub, shown in Forgot |
| Assets/Scripts/Login/LoginFeedback.cs | stub, `toast(message, is_error)` and `ask_guest_name(on_name)` only log for now |
| Assets/Resources/Frames/Login/LoginFrame.png | card art, 980x1230, cleaned from the uploaded frame |
| Assets/Resources/Icons/Login/IconEmail, IconLock, IconEye, IconEyeOff, IconClose .png | white 256px |

## reused, not edited

| file | used for |
| --- | --- |
| Assets/Scripts/Play/PlayUi.cs | `rect`, `image`, `text`, `stretch`, `place`, `pill`, `sprite`, `load_font` |
| Assets/Scripts/Play/PlayTheme.cs | gold, text, error, success, overlay_dark |
| Assets/Scripts/Play/PlayFocus.cs | `tick`, `lit`, `default_target`, `clear_selection` |
| Assets/Scripts/Lobby/LobbyUi.cs | `modal_blocker`, `blue_pill`, `rim_glow`, `place_fraction`, `no_transition`, `sanitize`, `PLAY_SCENE`, `CARET_WIDTH` |
| Assets/Resources/Buttons/Play/TabSelectedGold.png | gold plate for LOG IN (`LoginTheme.BUTTON_GOLD_PATH`) |
| Assets/Resources/Fonts/MatchaSalt.otf | via `PlayUi.load_font` |

## edited

| file | change |
| --- | --- |
| Assets/Scripts/Menu/MainMenuController.cs | `load_scene("Play")` goes through `LoginOverlay.require_login` |

## notes

- `LoginStore` keeps the refresh token in PlayerPrefs, which is plain text on disk. swap the body for a platform keystore before release.
- header slot is the top 28 percent of the inner rect (`LoginTheme.HEADER_RATIO`). in the mockup the first input starts at about 19 percent, so the body agent may want to change that ratio.
