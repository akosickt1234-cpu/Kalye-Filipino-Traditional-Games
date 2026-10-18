using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayer
{
    public string name;
    public bool ready;
    public bool is_host;
    public bool is_you;
    public int join_order;
    public string avatar_path;

    public bool is_empty
    {
        get { return string.IsNullOrEmpty(name); }
    }
}

public class LobbyFriend
{
    public string name;
    public bool online;
    public string avatar_path;
}

public class LobbyMessage
{
    public string name;
    public string text;
}

public static class LobbyState
{
    public const string JOIN_OK = "ok";
    public const string SYSTEM_NAME = "System";
    const string PLAY_SCENE = "Play";
    const string MATCH_SCENE = "Match";
    const string CODE_ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    const int CODE_GROUP_LENGTH = 4;
    const int MAX_TEAM_SIZE = 5;
    const int MAX_MESSAGES = 100;
    const int SEARCH_TARGET = 5;
    const float SEARCH_STEP_SECONDS = 1f;
    const string YOU_NAME = "Ruenz";

    static readonly List<LobbyPlayer> player_list = new List<LobbyPlayer>();
    static readonly List<LobbyFriend> friend_list = new List<LobbyFriend>();
    static readonly List<LobbyMessage> message_list = new List<LobbyMessage>();
    static LobbyStateRunner runner;
    static float search_timer;
    static int join_counter;

    public static bool in_lobby { get; private set; }
    public static string lobby_code { get; private set; } = string.Empty;
    public static int mode_index { get; private set; }
    public static bool searching { get; private set; }
    public static int opponents_found { get; private set; }
    public static bool is_host { get; private set; }

    public static event Action players_changed;
    public static event Action chat_changed;
    public static event Action mode_changed;
    public static event Action code_changed;
    public static event Action search_changed;

    public static IReadOnlyList<LobbyPlayer> players
    {
        get { return player_list; }
    }

    public static IReadOnlyList<LobbyFriend> friends
    {
        get { return friend_list; }
    }

    public static IReadOnlyList<LobbyMessage> messages
    {
        get { return message_list; }
    }

    public static LobbyPlayer you
    {
        get
        {
            for (var i = 0; i < player_list.Count; i++)
            {
                if (player_list[i].is_you) return player_list[i];
            }
            return null;
        }
    }

    public static PlayGameData current_game
    {
        get
        {
            var games = PlayState.games;
            if (games == null || games.Length == 0) return null;
            return games[Mathf.Clamp(mode_index, 0, games.Length - 1)];
        }
    }

    public static string mode_name
    {
        get
        {
            var game = current_game;
            return game != null ? game.display_name : string.Empty;
        }
    }

    public static int team_size
    {
        get
        {
            var game = current_game;
            var max_players = game != null ? game.max_players : 0;
            return Mathf.Max(1, Mathf.Min(MAX_TEAM_SIZE, max_players / 2));
        }
    }

    public static int player_count
    {
        get
        {
            var count = 0;
            for (var i = 0; i < player_list.Count; i++)
            {
                if (!player_list[i].is_empty) count++;
            }
            return count;
        }
    }

    public static void enter()
    {
        clear_listeners();
        if (in_lobby) return;

        in_lobby = true;
        lobby_code = generate();
        mode_index = clamp_mode(PlayState.selected_index);
        searching = false;
        opponents_found = 0;
        search_timer = 0f;
        join_counter = 0;

        build_mock_players();
        is_host = true;
        var self = you;
        if (self != null) self.is_host = true;
        fit_slots();
        build_mock_friends();
        build_mock_messages();
    }

    public static void minimize()
    {
        if (!in_lobby) return;
        LobbyUi.go_to_scene(PLAY_SCENE);
    }

    public static void leave()
    {
        searching = false;
        opponents_found = 0;
        search_timer = 0f;
        player_list.Clear();
        message_list.Clear();
        lobby_code = string.Empty;
        in_lobby = false;
        is_host = false;
        join_counter = 0;

        search_changed?.Invoke();
        players_changed?.Invoke();
        chat_changed?.Invoke();
        code_changed?.Invoke();
        mode_changed?.Invoke();
    }

    public static void clear_listeners()
    {
        players_changed = null;
        chat_changed = null;
        mode_changed = null;
        code_changed = null;
        search_changed = null;
    }

    public static string generate()
    {
        var chars = new char[CODE_GROUP_LENGTH * 2 + 1];
        for (var i = 0; i < chars.Length; i++) chars[i] = CODE_ALPHABET[UnityEngine.Random.Range(0, CODE_ALPHABET.Length)];
        chars[CODE_GROUP_LENGTH] = '-';
        return new string(chars);
    }

    public static string join_code(string value)
    {
        if (!in_lobby) return "You are not in a lobby";

        var code = normalize_code(value);
        if (string.IsNullOrEmpty(code)) return "Enter a lobby code";
        if (!is_valid_code(code)) return "Code must look like 7F3K-9Q2B";
        if (code == lobby_code) return "You are already in this lobby";

        var creator = earliest_other();
        if (creator == null) return "Lobby not found";

        var self = you;
        if (self != null) self.join_order = next_join_order();
        clear_host_flags();
        creator.is_host = true;
        is_host = false;
        lobby_code = code;
        players_changed?.Invoke();
        code_changed?.Invoke();
        return JOIN_OK;
    }

    public static bool regenerate_code()
    {
        if (!in_lobby || !is_host) return false;

        lobby_code = generate();
        code_changed?.Invoke();
        return true;
    }

    public static bool set_mode(int index)
    {
        if (!in_lobby || !is_host) return false;

        var next = clamp_mode(index);
        if (next == mode_index) return false;

        mode_index = next;
        fit_slots();
        mode_changed?.Invoke();
        players_changed?.Invoke();
        return true;
    }

    public static void set_host(bool value)
    {
        var self = you;
        if (self == null) return;

        var next_host = value ? self : earliest_other();
        if (next_host == null) return;

        clear_host_flags();
        next_host.is_host = true;
        is_host = next_host.is_you;
        players_changed?.Invoke();
    }

    public static bool remove_player(string name)
    {
        if (!in_lobby) return false;

        for (var i = 0; i < player_list.Count; i++)
        {
            var candidate = player_list[i];
            if (candidate.is_empty || candidate.is_you || candidate.name != name) continue;

            player_list[i] = new LobbyPlayer();
            if (candidate.is_host) assign_next_host();
            players_changed?.Invoke();
            return true;
        }
        return false;
    }

    public static bool has_player(string name)
    {
        for (var i = 0; i < player_list.Count; i++)
        {
            if (!player_list[i].is_empty && player_list[i].name == name) return true;
        }
        return false;
    }

    public static bool invite(LobbyFriend friend)
    {
        if (!in_lobby || friend == null || !friend.online) return false;
        if (has_player(friend.name)) return false;

        var slot = first_empty_slot();
        if (slot < 0)
        {
            add_message(SYSTEM_NAME, "The lobby is full");
            return false;
        }

        player_list[slot] = new LobbyPlayer { name = friend.name, ready = false, join_order = next_join_order(), avatar_path = friend.avatar_path };
        players_changed?.Invoke();
        add_message(SYSTEM_NAME, friend.name + " joined the lobby");
        return true;
    }

    public static void add_message(string name, string text)
    {
        var clean = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
        if (clean.Length == 0) return;

        message_list.Add(new LobbyMessage { name = name ?? string.Empty, text = clean });
        while (message_list.Count > MAX_MESSAGES) message_list.RemoveAt(0);
        chat_changed?.Invoke();
    }

    public static void start_search()
    {
        if (!in_lobby || searching) return;

        searching = true;
        opponents_found = 0;
        search_timer = 0f;
        ensure_runner();
        search_changed?.Invoke();
    }

    public static void cancel_search()
    {
        if (!searching) return;

        searching = false;
        opponents_found = 0;
        search_timer = 0f;
        search_changed?.Invoke();
    }

    internal static void tick(float delta_seconds)
    {
        if (!searching) return;

        search_timer += delta_seconds;
        if (search_timer < SEARCH_STEP_SECONDS) return;

        search_timer -= SEARCH_STEP_SECONDS;
        opponents_found = Mathf.Min(SEARCH_TARGET, opponents_found + 1);
        search_changed?.Invoke();
        if (opponents_found < SEARCH_TARGET) return;

        searching = false;
        search_timer = 0f;
        search_changed?.Invoke();
        if (Application.CanStreamedLevelBeLoaded(MATCH_SCENE)) LobbyUi.go_to_scene(MATCH_SCENE);
    }

    static void ensure_runner()
    {
        if (runner != null) return;

        var holder = new GameObject("lobby_state_runner");
        UnityEngine.Object.DontDestroyOnLoad(holder);
        runner = holder.AddComponent<LobbyStateRunner>();
    }

    static int clamp_mode(int index)
    {
        var games = PlayState.games;
        if (games == null || games.Length == 0) return 0;
        return Mathf.Clamp(index, 0, games.Length - 1);
    }

    static int first_empty_slot()
    {
        for (var i = 0; i < player_list.Count; i++)
        {
            if (player_list[i].is_empty) return i;
        }
        return -1;
    }

    static string normalize_code(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var code = value.Trim().ToUpperInvariant().Replace(" ", string.Empty);
        if (code.Length == CODE_GROUP_LENGTH * 2) code = code.Insert(CODE_GROUP_LENGTH, "-");
        return code;
    }

    static bool is_valid_code(string code)
    {
        if (code.Length != CODE_GROUP_LENGTH * 2 + 1) return false;

        for (var i = 0; i < code.Length; i++)
        {
            if (i == CODE_GROUP_LENGTH)
            {
                if (code[i] != '-') return false;
                continue;
            }
            if (CODE_ALPHABET.IndexOf(code[i]) < 0) return false;
        }
        return true;
    }

    static void fit_slots()
    {
        var size = team_size;

        while (player_list.Count > size)
        {
            var remove_at = last_removable_index(true);
            if (remove_at < 0) remove_at = last_removable_index(false);
            if (remove_at < 0) break;
            var removed = player_list[remove_at];
            player_list.RemoveAt(remove_at);
            if (removed.is_host) assign_next_host();
        }

        while (player_list.Count < size) player_list.Add(new LobbyPlayer());

        var has_host = false;
        for (var i = 0; i < player_list.Count; i++)
        {
            if (player_list[i].is_host) has_host = true;
        }

        if (!has_host) assign_next_host();
    }

    static int next_join_order()
    {
        join_counter++;
        return join_counter;
    }

    static void clear_host_flags()
    {
        for (var i = 0; i < player_list.Count; i++) player_list[i].is_host = false;
    }

    static LobbyPlayer earliest_player(bool include_you)
    {
        LobbyPlayer earliest = null;
        for (var i = 0; i < player_list.Count; i++)
        {
            var candidate = player_list[i];
            if (candidate.is_empty || (candidate.is_you && !include_you)) continue;
            if (earliest == null || candidate.join_order < earliest.join_order) earliest = candidate;
        }
        return earliest;
    }

    static LobbyPlayer earliest_other()
    {
        return earliest_player(false);
    }

    static void assign_next_host()
    {
        var next_host = earliest_player(true);
        if (next_host == null)
        {
            is_host = false;
            return;
        }

        clear_host_flags();
        next_host.is_host = true;
        is_host = next_host.is_you;
    }

    static int last_removable_index(bool keep_host)
    {
        for (var i = player_list.Count - 1; i >= 0; i--)
        {
            var candidate = player_list[i];
            if (candidate.is_you) continue;
            if (keep_host && candidate.is_host) continue;
            return i;
        }
        return -1;
    }

    static void build_mock_players()
    {
        var self = new LobbyPlayer { name = YOU_NAME, ready = true, is_you = true, join_order = next_join_order() };
        var first = new LobbyPlayer { name = "Bayanihan", ready = true, join_order = next_join_order() };
        var second = new LobbyPlayer { name = "Hazel", ready = true, join_order = next_join_order() };
        var third = new LobbyPlayer { name = "Charles", ready = false, join_order = next_join_order() };

        player_list.Clear();
        player_list.Add(first);
        player_list.Add(self);
        player_list.Add(second);
        player_list.Add(third);
        player_list.Add(new LobbyPlayer());
    }

    static void build_mock_friends()
    {
        friend_list.Clear();
        friend_list.Add(new LobbyFriend { name = "Hazel", online = true });
        friend_list.Add(new LobbyFriend { name = "Charles", online = true });
        friend_list.Add(new LobbyFriend { name = "Azey", online = true });
        friend_list.Add(new LobbyFriend { name = "Kae", online = false });
        friend_list.Add(new LobbyFriend { name = "Yohan", online = false });
    }

    static void build_mock_messages()
    {
        message_list.Clear();
        message_list.Add(new LobbyMessage { name = "Hazel", text = "Ready na ako!" });
        message_list.Add(new LobbyMessage { name = "Charles", text = "Wait lang, mag-aayos pa." });
    }
}

public class LobbyStateRunner : MonoBehaviour
{
    void Update()
    {
        LobbyState.tick(Time.unscaledDeltaTime);
    }
}
