# Kalye-Filipino-Traditional-Games
## Backend

The REST backend (accounts, lobbies, chat, friends, match records) is in `Backend/`. See [Backend/README.md](Backend/README.md) for setup and usage.

## Main menu

`Assets/Scenes/Menu/Menu.unity` is the main menu scene (scripts in `Assets/Scripts/Menu`, art in `Assets/Resources/Background/Menu` and `Assets/Resources/Buttons/Menu`, fonts in `Assets/Resources/Fonts`). It shows the background art, logo and Play / Leaderboard / Edit Avatar / Settings. Menu items load scenes by name through `MainMenuController.load_scene`.

Scenes: `Play` is new and in the build settings. `Lobby` stays in the build settings but is no longer linked from the menu. `Leaderboard`, `EditAvatar` and `Settings` do not exist yet and log a warning.

The play screen (game select) is opened by Play. Its scripts are in `Assets/Scripts/Play` and its art is in `Assets/Resources/<Type>/Play`. All ui is built in code at runtime.

## Lobby

`Assets/Scenes/Lobby/Lobby.unity` is the lobby, opened from the Play screen (scripts in `Assets/Scripts/Lobby`). It has a top bar, player slots, chat, a player count and mode picker, a lobby code with copy and join, a friends panel and a start search bar. Host only actions follow `LobbyState.is_host`. Minimize returns to Play, where the Play button becomes Return Lobby until the lobby is left. `PrivateLobby` and `Match` scenes do not exist yet and are not in the build settings.

After pulling, open the project in Unity once and commit the generated `.meta` files.

The menu font (Matcha Salt, MJType) is licensed for personal use only; get a commercial license before shipping.

## Login

Play asks for login first. `MainMenuController.load_scene("Play")` goes through `LoginOverlay.require_login`, which runs the scene load right away when a session exists, restores a saved session with the stored refresh token, or opens the login overlay. The overlay is a floating card built in code on top of the Menu scene (scripts in `Assets/Scripts/Login`, art in `Assets/Resources/Frames/Login` and `Assets/Resources/Icons/Login`). It has sign in, register, forgot password and guest modes, a blurred backdrop, toasts with retry, and full keyboard use (tab cycles inside the card, escape closes).

The auth api base url is `LoginService.BASE_URL` (`/auth/login`, `/auth/guest`, `/auth/register`, `/auth/refresh`, `/auth/logout`). Enable `Allow downloads over HTTP` in player settings while the base url is http. The refresh token is kept in an encrypted file under `Application.persistentDataPath`, which is weak on non windows platforms; use a platform keystore before release. Forgot password is not available yet.

Shared and feedback resources are listed in `login_shared_resources.md` and `login_feedback_resources.md`.

## Version history

- v0.1: lobby screen with chat, mode picker, lobby code, friends panel, start search, and a Return Lobby button on Play
