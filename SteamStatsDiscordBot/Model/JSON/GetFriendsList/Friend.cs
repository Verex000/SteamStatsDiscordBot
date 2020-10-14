using Newtonsoft.Json;

namespace SteamStatsDiscordBot.Model.JSON.GetFriendsList
{
    public class Friend
    {
        [JsonProperty("steamid")]
        public string steamId { get; set; }
        [JsonProperty("relationship")]
        public string relationship { get; set; }
        [JsonProperty("friend_since")]
        public int friend_since { get; set; }
    }
}