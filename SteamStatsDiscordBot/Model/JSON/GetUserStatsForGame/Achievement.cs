using Newtonsoft.Json;

namespace SteamStatsDiscordBot.Model.JSON.GetUserStatsForGame
{
    public class Achievement
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("achieved")]
        public int achieved { get; set; }
    }
}