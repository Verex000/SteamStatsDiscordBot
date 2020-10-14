using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamStatsDiscordBot.Model.JSON.GetAppList;
using SteamStatsDiscordBot.Model.JSON.GetFriendsList;
using SteamStatsDiscordBot.Model.JSON.GetIdByUserName;
using SteamStatsDiscordBot.Model.JSON.GetPlayerSummaries;
using SteamStatsDiscordBot.Model.JSON.GetUserStatsForGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SteamStatsDiscordBot.Commands
{
    class SteamCommands : IModule
    {
        private IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")))
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                    .Build();

        private static readonly HttpClient client = new HttpClient();

        private List<App> apps = null;

        private JsonSerializer json = new JsonSerializer();

        //Among us app id = 945360
        [Command("concurrent_players")]
        [Aliases("c", "players_on")]
        [Description("Returns the current amount of active players.")]
        public async Task ConcurrentPlayers(CommandContext ctx, [RemainingText] string appid)
        {
            await ctx.TriggerTypingAsync();
            var msg = await getNumberOfConcurrentPlayers(appid.ToLower());
            await ctx.RespondAsync($"{ctx.User.Mention} {msg}");
        }

        [Command("user_stats")]
        [Aliases("u", "stats")]
        [Description("Returns a player's achievements and statistics for a given game.")]
        public async Task UserStats(CommandContext ctx, string steamId, [RemainingText] string appId)
        {
            await ctx.TriggerTypingAsync();
            var msg = await getUserStatsForGame(appId, steamId);
            Stream s = new MemoryStream(Encoding.UTF8.GetBytes(msg));
            await ctx.RespondAsync($"{ctx.User.Mention} Here are your achievements and statistics for {appId}!");
            await ctx.RespondWithFileAsync(s, $"{ctx.User.Username}_stats.json");
        }

        [Command("wishlist")]
        [Aliases("w", "wl")]
        [Description("Returns a list of a player's wishlist")]
        public async Task Wishlist(CommandContext ctx, string steamId) 
        {
            var msg = await getUserWishlist(steamId);
        }

        [Command("friends")]
        [Aliases("f", "fr")]
        [Description("Returns a list of a player's friendslist")]
        public async Task Friends(CommandContext ctx, string steamId)
        {
            await ctx.TriggerTypingAsync();
            var msg = await getFriendsList(steamId);
            if (msg.Equals("Profile is not public, or request has failed.")) 
            {
                await ctx.RespondAsync($"{ctx.User.Mention} Profile is not public, or request has failed!");
            }
            else
            {
                Stream s = new MemoryStream(Encoding.UTF8.GetBytes(msg));
                await ctx.RespondAsync($"{ctx.User.Mention} Here is the friends list of  {steamId}!");
                await ctx.RespondWithFileAsync(s, $"{steamId}_friends_list.json");
            }
        }

        private async Task<string> getNumberOfConcurrentPlayers(string appid)
        {
            if (!appid.All(char.IsDigit))
            {
                appid = await getIdByName(appid);
            }
            if (appid == null || !appid.All(char.IsDigit))
            {
                return $"Invalid app id or app name, try again";
            }
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = client.GetStringAsync($"https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1?appid={appid}");

            var response = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(await stringTask);
            var playerCount = response["response"]["player_count"];
            var result = response["response"]["result"];
            if (int.Parse(result) == 1)
            {
                return $"There are {playerCount} players on right now!";
            }
            else
            {
                return $"API request failed, try again.";
            }
        }

        // GET https://api.steampowered.com/ISteamApps/GetAppList/v2/
        private async Task<string> getIdByName(string appName)
        {
            if(apps == null)
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var stringTask = client.GetStringAsync($"https://api.steampowered.com/ISteamApps/GetAppList/v2/");
                var response = JsonConvert.DeserializeObject<AppListRoot>(await stringTask);
                apps = response.appList.apps;
            }
            var one = apps.FirstOrDefault<App>(app => app.name.ToLower() == appName.ToLower());
            if (one == null)
            {
                return null;
            }
            return one.appId.ToString();

        }

        private async Task<string> getSteamIdByName(string steamUserName)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var url = config.GetValue<string>("steam:GetIdByUserName");
            url = url.Replace("<UserName>", steamUserName);

            var stringTask = client.GetStringAsync(url);
            try
            {
                var response = JsonConvert.DeserializeObject<SteamProfile>(await stringTask);
                return response.steamID;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private async Task<string> getUserStatsForGame(string appId, string steamId) 
        {
            if (!appId.All(char.IsDigit))
            {
                appId = await getIdByName(appId);
                if (!appId.All(char.IsDigit) || appId == null)
                {
                    return $"Invalid app id or app name, try again.";
                }
            }
            if (!steamId.All(char.IsDigit) || appId == null)
            {
                steamId = await getSteamIdByName(steamId);
                if (!steamId.All(char.IsDigit))
                {
                    return steamId;
                }
            }
            var url = config.GetValue<string>("steam:GetUserStatsForGame");
            var apiKey = config.GetValue<string>("steam:apiKey");
            url = url.Replace("<appId>", appId);
            url = url.Replace("<key>", apiKey);
            url = url.Replace("<steamId>", steamId);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var stringTask = client.GetStringAsync(url);
            try
            {
                var res = JsonConvert.DeserializeObject<PlayerStatsRoot>(await stringTask);
                var statsRoot = res.playerStats;
                var stringRes = new { Achievements = res.playerStats.achievements, Stats = res.playerStats.stats };
                return JsonConvert.SerializeObject(stringRes, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return "Profile is not public, or request has failed.";
            }
        }

        public async Task<string> getUserWishlist(string steamId)
        {
            List<Tuple<string, DateTime>> gameDateList = new List<Tuple<string, DateTime>>();
            var url = "https://store.steampowered.com/wishlist/profiles/<steamId>/wishlistdata/";

            url = url.Replace("<steamId>", steamId);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var stringTask = client.GetStringAsync(url);
            try
            {
                var res = await stringTask;
                var jObj = JObject.Parse(res);
                foreach (var x in jObj)
                {
                    var appId = x.Key;
                    JToken info = x.Value;
                    var name = (string)info["name"];
                    var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)info["added"]);
                    var dateAdded = dateTimeOffset.DateTime;
                    gameDateList.Add(new Tuple<string, DateTime>(name, dateAdded));
                    
                }
                //Dictionary<string, DateTime> dict = new Dictionary<string, DateTime>();
                gameDateList.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                IEnumerable<Tuple<string, DateTime>> mostRecent = gameDateList.TakeLast(10).Reverse();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // EXAMPLE: http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key=74E7C9D8B13DEDC97486FAD5380B9C62&steamid=76561198190382628&relationship=friend
        public async Task<string> getFriendsList(string steamId)
        {
            if(!steamId.All(char.IsDigit))
            {
                steamId = await getSteamIdByName(steamId);
            }
            var url = config.GetValue<string>("steam:GetFriendsList");
            var apiKey = config.GetValue<string>("steam:apiKey");
            url = url.Replace("<key>", apiKey);
            url = url.Replace("<steamId>", steamId);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var stringTask = client.GetStringAsync(url);
            try
            {
                var res = JsonConvert.DeserializeObject<FriendsListRoot>(await stringTask);
                var friendsList = res.friendsList.friends;
                List<Object> objList = new List<object>();
                List<string> steamFriendsIdList = new List<string>();

                friendsList.ForEach((friend) => {
                    steamFriendsIdList.Add(friend.steamId);
                });

                var playerList = await getPlayerSummary(steamFriendsIdList);

                playerList.ForEach((player) => {
                    var obj = new { id = player.steamId, name = player.personaName };
                    objList.Add(obj);
                });

                return JsonConvert.SerializeObject(objList, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return "Profile is not public, or request has failed.";
            }
        }

        // EXAMPLE: http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=74E7C9D8B13DEDC97486FAD5380B9C62&steamids=76561198190382628
        public async Task<List<Player>> getPlayerSummary(List<string> steamIds) 
        {
            var url = config.GetValue<string>("steam:GetPlayerSummaries");
            var apiKey = config.GetValue<string>("steam:apiKey");
            url = url.Replace("<key>", apiKey);
            steamIds.ForEach((id) => { url += (id + ","); });
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var stringTask = client.GetStringAsync(url);
            try
            {
                var res = JsonConvert.DeserializeObject<SummaryRoot>(await stringTask);
                var playerList = res.summary.playerList;
                return playerList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
