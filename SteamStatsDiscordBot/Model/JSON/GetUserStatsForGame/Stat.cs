using Newtonsoft.Json;

namespace SteamStatsDiscordBot.Model.JSON.GetUserStatsForGame
{
    public class Stat
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("value")]
        public int value { get; set; }
    }
}