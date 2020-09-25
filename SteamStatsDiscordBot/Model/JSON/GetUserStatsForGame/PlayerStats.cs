using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamStatsDiscordBot.Model.JSON.GetUserStatsForGame
{
    public class PlayerStats
    {
        [JsonProperty("steamID")]
        public string steamId { get; set; }
        [JsonProperty("gameName")]
        public string gameName { get; set; }
        [JsonProperty("achievements")]
        public List<Achievement> achievements { get; set; }
        [JsonProperty("stats")]
        public List<Stat> stats { get; set; }
    }
}