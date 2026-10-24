using System;
using UnityEngine;

public static class PlayProfile
{
    const float CACHE_SECONDS = 60f;
    const string PATH_ME = "/me";
    const string PATH_STATS = "/me/stats";
    const string LOAD_FAILED_MESSAGE = "could not load your profile";
    const int GUEST_LEVEL = 1;
    const int UNKNOWN_LEVEL = 0;

    public static string username { get; private set; } = string.Empty;
    public static string avatar_id { get; private set; } = string.Empty;
    public static int level { get; private set; } = UNKNOWN_LEVEL;
    public static bool is_loading { get; private set; }
    public static bool has_loaded { get; private set; }
    public static float loaded_at { get; private set; }

    static string loaded_for = string.Empty;
    static int load_serial;

    public static event Action changed;

    public static int level_from_played(int played)
    {
        return 1 + Mathf.Max(0, played);
    }

    public static void load(bool force)
    {
        if (is_loading) return;
        if (!LoginSession.is_logged_in) return;

        var owner = LoginSession.display_name ?? string.Empty;
        if (has_loaded && owner != loaded_for) clear_values();

        if (!force && has_loaded && Time.realtimeSinceStartup - loaded_at < CACHE_SECONDS) return;

        is_loading = true;
        load_serial++;
        var serial = load_serial;
        var guest = LoginSession.is_guest;

        if (!string.IsNullOrWhiteSpace(owner)) username = owner;
        changed?.Invoke();

        ApiClient.get<ApiModels.MeReply>(PATH_ME,
            reply => on_me(reply, guest, owner, serial),
            error => on_fail(error, serial));
    }

    static void on_me(ApiModels.MeReply reply, bool guest, string owner, int serial)
    {
        if (serial != load_serial) return;

        var user = reply != null ? reply.user : null;
        if (user != null)
        {
            if (!string.IsNullOrWhiteSpace(user.username)) username = user.username;
            avatar_id = user.avatar ?? string.Empty;
        }

        if (guest || (user != null && user.is_guest))
        {
            level = GUEST_LEVEL;
            finish(owner);
            return;
        }

        changed?.Invoke();

        ApiClient.get<ApiModels.StatsReply>(PATH_STATS,
            stats =>
            {
                if (serial != load_serial) return;
                var played = stats != null && stats.total != null ? stats.total.played : 0;
                level = level_from_played(played);
                finish(owner);
            },
            error => on_fail(error, serial));
    }

    static void finish(string owner)
    {
        is_loading = false;
        has_loaded = true;
        loaded_at = Time.realtimeSinceStartup;
        loaded_for = owner;
        changed?.Invoke();
    }

    static void on_fail(ApiError error, int serial)
    {
        if (serial != load_serial) return;

        is_loading = false;

        if (error != null && error.session_ended) return;

        if (level <= UNKNOWN_LEVEL) level = GUEST_LEVEL;
        changed?.Invoke();

        ApiToast.toast_retry(LOAD_FAILED_MESSAGE, () => load(true));
    }

    static void clear_values()
    {
        username = string.Empty;
        avatar_id = string.Empty;
        level = UNKNOWN_LEVEL;
        has_loaded = false;
        loaded_at = 0f;
        loaded_for = string.Empty;
    }
}
