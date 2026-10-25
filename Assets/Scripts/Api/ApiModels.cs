using System;

public static class ApiModels
{
    [Serializable]
    public class MeUser
    {
        public string id;
        public string username;
        public bool is_guest;
        public string avatar;
    }

    [Serializable]
    public class MeReply
    {
        public MeUser user;
    }

    [Serializable]
    public class StatsCounts
    {
        public int played;
        public int wins;
        public int losses;
        public int draws;
    }

    [Serializable]
    public class StatsGame
    {
        public string game_type;
        public int played;
        public int wins;
        public int losses;
        public int draws;
    }

    [Serializable]
    public class StatsReply
    {
        public StatsCounts total;
        public StatsGame[] games;
    }
}
