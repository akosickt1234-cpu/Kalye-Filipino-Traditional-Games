public static class LoginSession
{
    public static bool is_logged_in { get; private set; }
    public static bool is_guest { get; private set; }
    public static string access_token { get; private set; } = string.Empty;
    public static string refresh_token { get; private set; } = string.Empty;
    public static string display_name { get; private set; } = string.Empty;

    public static void set(string name, string access, string refresh, bool guest)
    {
        display_name = name ?? string.Empty;
        access_token = access ?? string.Empty;
        refresh_token = refresh ?? string.Empty;
        is_guest = guest;
        is_logged_in = true;
    }

    public static void clear()
    {
        is_logged_in = false;
        is_guest = false;
        access_token = string.Empty;
        refresh_token = string.Empty;
        display_name = string.Empty;
    }
}
