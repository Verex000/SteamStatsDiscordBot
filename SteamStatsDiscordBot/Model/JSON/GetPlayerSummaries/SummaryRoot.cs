using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetPlayerSummaries
{
    class SummaryRoot
    {
        [JsonProperty("response")]
        public Summary summary { get; set; }
    }
}
