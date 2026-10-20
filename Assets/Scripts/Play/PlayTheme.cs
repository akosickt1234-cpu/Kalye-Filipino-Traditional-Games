using UnityEngine;

public static class PlayTheme
{
    public static readonly Color gold = new Color(0.914f, 0.761f, 0.478f, 1f);
    public static readonly Color gold_soft = new Color(0.965f, 0.824f, 0.596f, 1f);
    public static readonly Color glow = new Color(0.945f, 0.788f, 0.510f, 0.35f);
    public static readonly Color text = new Color(0.996f, 0.945f, 0.812f, 1f);
    public static readonly Color text_muted = new Color(0.745f, 0.690f, 0.580f, 1f);
    public static readonly Color text_white = new Color(1f, 1f, 1f, 1f);
    public static readonly Color icon = new Color(1f, 0.949f, 0.827f, 1f);
    public static readonly Color shadow = new Color(0f, 0f, 0f, 0.7f);
    public static readonly Color panel_dark = new Color(0.114f, 0.125f, 0.200f, 1f);
    public static readonly Color clear = new Color(0f, 0f, 0f, 0f);
    public static readonly Color white = new Color(1f, 1f, 1f, 1f);
    public static readonly Color panel_blue = new Color(0.067f, 0.102f, 0.180f, 1f);
    public static readonly Color fill_soft = new Color(panel_blue.r, panel_blue.g, panel_blue.b, 0.40f);
    public static readonly Color magic_glow = new Color(1f, 0.937f, 0.780f, 0.6f);
    public static readonly Color magic_spark = new Color(0.949f, 0.812f, 0.545f, 1f);
    public static readonly Color overlay_dark = new Color(0.034f, 0.038f, 0.060f, 1f);
    public static readonly Color label_brown = new Color(0.274f, 0.152f, 0.048f, 1f);
    public static readonly Color success = new Color(0.30f, 0.78f, 0.42f, 1f);
    public static readonly Color error = new Color(0.90f, 0.35f, 0.35f, 1f);

    public static readonly Color lobby_panel_bg = from_hex("#062647");
    public static readonly Color lobby_inner_bg = from_hex("#0A3152");
    public static readonly Color lobby_edge_shadow = from_hex("#031326");
    public static readonly Color[] lobby_panel_stops = { from_hex("#0B3558"), from_hex("#062544"), from_hex("#03152C") };
    public static readonly Color slot_bg = from_hex("#082A49");
    public static readonly Color slot_border = from_hex("#397BAA");
    public static readonly Color[] slot_stops = { from_hex("#104269"), from_hex("#092C4B"), from_hex("#051A30") };
    public static readonly Color border_highlight = from_hex("#8BC9F0");
    public static readonly Color[] border_cyan_stops = { from_hex("#164B72"), from_hex("#8BC9F0"), from_hex("#2D709D") };
    public static readonly Color lobby_text = from_hex("#F3F5F8");
    public static readonly Color lobby_text_muted = from_hex("#9BBBDC");
    public static readonly Color lobby_gold_icon = from_hex("#F4D58B");
    public static readonly Color lobby_ready = from_hex("#36D98B");
    public static readonly Color[] start_stops = { from_hex("#FFF0B0"), from_hex("#F6CD72"), from_hex("#E8A943"), from_hex("#A96D27") };
    public static readonly Color start_text = from_hex("#563516");
    public static readonly Color start_border = from_hex("#FFE6A0");
    public static readonly Color start_glow = with_alpha(from_hex("#FFD982"), 0.45f);
    public static readonly Color[] trim_primary_stops = { from_hex("#8D602D"), from_hex("#D7AC5D"), from_hex("#FFF0B0"), from_hex("#D8AD60"), from_hex("#8D602D") };
    public static readonly Color[] trim_secondary_stops = { from_hex("#4D351F"), from_hex("#A87B3D"), from_hex("#FBE3A0"), from_hex("#A87B3D"), from_hex("#4D351F") };
    public static readonly Color[] pill_stops = { from_hex("#164267"), from_hex("#092C49"), from_hex("#04182D") };
    public static readonly Color pill_border = from_hex("#5B91B7");
    public static readonly Color pill_text = from_hex("#D9EEFF");
    public static readonly Color title_accent = from_hex("#FFE6A0");
    public static readonly Color avatar_ring = from_hex("#75B8E8");
    public static readonly Color plus_icon = from_hex("#D9EEFF");
    public static readonly Color crown = from_hex("#FFF0A8");
    public static readonly Color friends_border = from_hex("#76BDE9");
    public static readonly Color friends_icon = from_hex("#7FAED9");
    public static readonly Color divider_line = from_hex("#4C87AE");
    public static readonly Color code_capsule = from_hex("#0A3152");
    public static readonly Color info_icon = from_hex("#A9D5F5");
    public static readonly Color slot_glow = with_alpha(from_hex("#5AB8F0"), 0.17f);
    public static readonly Color panel_inner_glow = with_alpha(from_hex("#2674A3"), 0.14f);

    public static Color from_hex(string value)
    {
        Color parsed;
        return ColorUtility.TryParseHtmlString(value, out parsed) ? parsed : Color.magenta;
    }

    public static Color with_alpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
    }
}
