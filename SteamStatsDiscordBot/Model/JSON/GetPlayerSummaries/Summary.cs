using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetPlayerSummaries
{
    public class Summary
    {
        [JsonProperty("players")]
        public List<Player> playerList { get; set; }

    }
}