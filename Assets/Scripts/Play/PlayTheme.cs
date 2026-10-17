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
    public static readonly Color fill_soft = new Color(panel_blue.r, panel_blue.g, panel_blue.b, 0.20f);
    public static readonly Color magic_glow = new Color(1f, 0.937f, 0.780f, 0.6f);
    public static readonly Color magic_spark = new Color(0.949f, 0.812f, 0.545f, 1f);
    public static readonly Color overlay_dark = new Color(0.034f, 0.038f, 0.060f, 1f);
    public static readonly Color label_brown = new Color(0.274f, 0.152f, 0.048f, 1f);

    public static Color with_alpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
    }
}
