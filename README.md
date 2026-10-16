# Kalye-Filipino-Traditional-Games
## Backend

The REST backend (accounts, lobbies, chat, friends, match records) is in `Backend/`. See [Backend/README.md](Backend/README.md) for setup and usage.

## Main menu

`Assets/Scenes/Menu/Menu.unity` is the main menu scene (its scripts are in `Assets/Scripts/Menu`; art is in `Assets/Resources/Background/Menu` and `Assets/Resources/Buttons/Menu`, fonts in `Assets/Resources/Fonts`). It shows the background art, logo and Lobby / Private Lobby / Edit Avatar / Settings. Menu items load scenes by name through `MainMenuController.load_scene`. `Lobby` is in the build settings; `PrivateLobby`, `EditAvatar` and `Settings` log a warning until those scenes exist. The menu font (Matcha Salt, MJType) is licensed for personal use only; get a commercial license before shipping.
