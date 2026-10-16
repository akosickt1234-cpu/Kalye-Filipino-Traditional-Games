using System;
using UnityEngine;

public static class PlayState
{
    const string PLAYER_NAME_KEY = "player_name";
    const string DEFAULT_PLAYER_NAME = "Guest";
    const int DEFAULT_PLAYER_LEVEL = 12;

    public static PlayGameData[] games = PlayGameData.create_defaults();
    public static int selected_index;
    public static string player_name = DEFAULT_PLAYER_NAME;
    public static int player_level = DEFAULT_PLAYER_LEVEL;

    public static event Action<int> selection_changed;

    public static PlayGameData current
    {
        get
        {
            if (games == null || games.Length == 0) return null;
            return games[Mathf.Clamp(selected_index, 0, games.Length - 1)];
        }
    }

    public static void select(int index)
    {
        if (games == null || games.Length == 0) return;

        selected_index = Mathf.Clamp(index, 0, games.Length - 1);
        selection_changed?.Invoke(selected_index);
    }

    public static void reset()
    {
        selection_changed = null;
        games = PlayGameData.create_defaults();
        selected_index = 0;
        player_level = DEFAULT_PLAYER_LEVEL;

        var saved_name = PlayerPrefs.GetString(PLAYER_NAME_KEY, string.Empty);
        player_name = string.IsNullOrWhiteSpace(saved_name) ? DEFAULT_PLAYER_NAME : saved_name;
    }
}
