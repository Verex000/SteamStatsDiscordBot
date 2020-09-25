using Newtonsoft.Json;

namespace SteamStatsDiscordBot.Model.JSON.GetAppList
{
    public class App
    {
        [JsonProperty("appid")]
        public int appId { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
    }
}