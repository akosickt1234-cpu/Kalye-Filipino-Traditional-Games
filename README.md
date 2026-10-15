# Kalye-Filipino-Traditional-Games
## Backend

The REST backend (accounts, lobbies, chat, friends, match records) is in `backend/`. See [backend/README.md](backend/README.md) for setup and usage.

## Main menu

`Assets/Menu/menu.unity` is the main menu scene (scripts live beside it in `Assets/Menu`; art is in `Assets/bg` and `Assets/buttons`, fonts in `Assets/fonts`). It shows the background art, logo and Lobby / Private Lobby / Edit Avatar / Settings. Menu items load scenes by name through `MainMenuController.load_scene`. `lobby` is in the build settings; `private_lobby`, `edit_avatar` and `settings` log a warning until those scenes exist. The menu font (Matcha Salt, MJType) is licensed for personal use only; get a commercial license before shipping.
