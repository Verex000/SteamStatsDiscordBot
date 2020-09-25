using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetAppList
{
    class AppListRoot
    {
        [JsonProperty("applist")]
        public AppList appList { get; set; }
    }
}
