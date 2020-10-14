using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamStatsDiscordBot.Model.JSON.GetPlayerSummaries
{
    public class Player
    {
        [JsonProperty("steamid")]
        public string steamId { get; set; }

        [JsonProperty("communityvisibilitystate")]
        public int Communityvisibilitystate { get; set; }

        [JsonProperty("profilestate")]
        public int Profilestate { get; set; }

        [JsonProperty("personaname")]
        public string personaName { get; set; }

        [JsonProperty("profileurl")]
        public string profileUrl { get; set; }

        [JsonProperty("avatar")]
        public string avatar { get; set; }

        [JsonProperty("avatarmedium")]
        public string Avatarmedium { get; set; }

        [JsonProperty("avatarfull")]
        public string Avatarfull { get; set; }

        [JsonProperty("avatarhash")]
        public string Avatarhash { get; set; }

        [JsonProperty("lastlogoff")]
        public int Lastlogoff { get; set; }

        [JsonProperty("personastate")]
        public int Personastate { get; set; }

        [JsonProperty("primaryclanid")]
        public string Primaryclanid { get; set; }

        [JsonProperty("timecreated")]
        public int Timecreated { get; set; }

        [JsonProperty("personastateflags")]
        public int Personastateflags { get; set; }
    }
}