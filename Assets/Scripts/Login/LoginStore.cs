using UnityEngine;

public static class LoginStore
{
    const string KEY = "login.refresh_token";

    public static void save(string refresh_token)
    {
        if (string.IsNullOrEmpty(refresh_token))
        {
            clear();
            return;
        }

        PlayerPrefs.SetString(KEY, refresh_token);
        PlayerPrefs.Save();
    }

    public static string load()
    {
        return PlayerPrefs.GetString(KEY, string.Empty);
    }

    public static void clear()
    {
        if (!PlayerPrefs.HasKey(KEY)) return;

        PlayerPrefs.DeleteKey(KEY);
        PlayerPrefs.Save();
    }
}
