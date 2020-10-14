using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetFriendsList
{
    class FriendsList
    {
        [JsonProperty("friends")]
        public List<Friend> friends { get; set; }
    }
}
