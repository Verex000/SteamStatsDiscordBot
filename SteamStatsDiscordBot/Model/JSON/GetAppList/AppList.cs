using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetAppList
{
    class AppList
    {
        [JsonProperty("apps")]
        public List<App> apps { get; set; }
    }
}
