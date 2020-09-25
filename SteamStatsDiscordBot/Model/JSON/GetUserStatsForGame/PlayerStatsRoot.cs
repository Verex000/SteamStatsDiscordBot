using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetUserStatsForGame
{
    class PlayerStatsRoot
    {
        [JsonProperty("playerstats")]
        public PlayerStats playerStats { get; set; }

    }
}
