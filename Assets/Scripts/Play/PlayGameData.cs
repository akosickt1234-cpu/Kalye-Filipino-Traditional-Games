public class PlayGameData
{
    public string id;
    public string display_name;
    public string description;
    public string thumb_path;
    public string bg_path;
    public int min_players;
    public int max_players;

    public static PlayGameData[] create_defaults()
    {
        return new[]
        {
            new PlayGameData
            {
                id = "patintero",
                display_name = "Patintero",
                description = "Work with your team to block the runners and prevent them from reaching the base. Be quick, be smart, be the last one standing!",
                thumb_path = "Cards/Play/CardThumbPatintero",
                bg_path = "Background/Play/BgPatintero2K",
                min_players = 4,
                max_players = 8
            },
            new PlayGameData
            {
                id = "chinese_garter",
                display_name = "Chinese Garter",
                description = "Jump over the rising rubber band without touching it. Clear every height to win the round.",
                thumb_path = "Cards/Play/CardThumbChineseGarter",
                bg_path = "Background/Play/BgChineseGarter2K",
                min_players = 3,
                max_players = 6
            },
            new PlayGameData
            {
                id = "sipa",
                display_name = "Sipa",
                description = "Keep the sipa in the air using only your feet. Do not let it drop!",
                thumb_path = "Cards/Play/CardThumbSipa",
                bg_path = "Background/Play/BgSipa2K",
                min_players = 2,
                max_players = 6
            },
            new PlayGameData
            {
                id = "tumbang_preso",
                display_name = "Tumbang Preso",
                description = "Knock down the can, then run for cover before the guard tags you.",
                thumb_path = "Cards/Play/CardThumbTumbangPreso",
                bg_path = "Background/Play/BgTumbangPreso2K",
                min_players = 4,
                max_players = 8
            }
        };
    }
}
