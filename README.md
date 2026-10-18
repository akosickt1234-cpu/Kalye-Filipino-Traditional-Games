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

## Version history

- v0.1: lobby screen with chat, mode picker, lobby code, friends panel, start search, and a Return Lobby button on Play
