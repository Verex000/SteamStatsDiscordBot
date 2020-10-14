using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetFriendsList
{
    class FriendsListRoot
    {
        [JsonProperty("friendslist")]

        public FriendsList friendsList { get; set; }
    }
}
