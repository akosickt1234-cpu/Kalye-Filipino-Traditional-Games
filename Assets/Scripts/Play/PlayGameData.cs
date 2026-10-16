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
                min_players = 10,
                max_players = 12
            },
            new PlayGameData
            {
                id = "chinese_garter",
                display_name = "Chinese Garter",
                description = "Clear the rising garter across seven rounds, from Super Easy to Impossible. Land clean jumps to score the most points.",
                thumb_path = "Cards/Play/CardThumbChineseGarter",
                bg_path = "Background/Play/BgChineseGarter2K",
                min_players = 2,
                max_players = 10
            },
            new PlayGameData
            {
                id = "sipa",
                display_name = "Sipa",
                description = "Face one opponent and keep the sipa in the air. Kick it where they cannot reach it and win the best of three rounds.",
                thumb_path = "Cards/Play/CardThumbSipa",
                bg_path = "Background/Play/BgSipa2K",
                min_players = 2,
                max_players = 2
            },
            new PlayGameData
            {
                id = "tumbang_preso",
                display_name = "Tumbang Preso",
                description = "Knock down the lata for 15 points and get back to base safely for 6. The Taya wins by tagging everyone, or the top scorer wins after 300 seconds.",
                thumb_path = "Cards/Play/CardThumbTumbangPreso",
                bg_path = "Background/Play/BgTumbangPreso2K",
                min_players = 4,
                max_players = 12
            }
        };
    }
}
