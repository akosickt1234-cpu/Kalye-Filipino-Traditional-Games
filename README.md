# Kalye-Filipino-Traditional-Games
## Backend

The REST backend (accounts, lobbies, chat, friends, match records) is in `Backend/`. See [Backend/README.md](Backend/README.md) for setup and usage.

## Main menu

`Assets/Scenes/Menu/Menu.unity` is the main menu scene (scripts in `Assets/Scripts/Menu`, art in `Assets/Resources/Background/Menu` and `Assets/Resources/Buttons/Menu`, fonts in `Assets/Resources/Fonts`). It shows the background art, logo and Play / Leaderboard / Edit Avatar / Settings. Menu items load scenes by name through `MainMenuController.load_scene`.

Scenes: `Play` is new and in the build settings. `Lobby` stays in the build settings but is no longer linked from the menu. `Leaderboard`, `EditAvatar` and `Settings` do not exist yet and log a warning.

The play screen (game select) is opened by Play. Its scripts are in `Assets/Scripts/Play` and its art is in `Assets/Resources/<Type>/Play`. All ui is built in code at runtime.

After pulling, open the project in Unity once and commit the generated `.meta` files.

The menu font (Matcha Salt, MJType) is licensed for personal use only; get a commercial license before shipping.
