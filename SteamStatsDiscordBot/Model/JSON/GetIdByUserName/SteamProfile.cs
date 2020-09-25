using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetIdByUserName
{
    class SteamProfile
    {
        [JsonProperty("steamID64")]
        public string steamID { get; set; }
        [JsonProperty("steamID")]
        public string steamName { get; set; }
        [JsonProperty("onlineState")]
        public string onlineState { get; set; }
        [JsonProperty("stateMessage")]
        public string stateMessage { get; set; }
        [JsonProperty("privacyState")]
        public string privacyState { get; set; }
        [JsonProperty("visibilityState")]
        public string visibilityState { get; set; }
        [JsonProperty("avatarIcon")]
        public string avatarIcon { get; set; }
        [JsonProperty("avatarMedium")]
        public string avatarMedium { get; set; }
        [JsonProperty("avatarFull")]
        public string avatarFull { get; set; }
        [JsonProperty("vacBanned")]
        public string vacBanned { get; set; }
        [JsonProperty("tradeBanState")]
        public string tradeBanState { get; set; }
        [JsonProperty("isLimitedAccount")]
        public string isLimitedAccount { get; set; }
    }
}
