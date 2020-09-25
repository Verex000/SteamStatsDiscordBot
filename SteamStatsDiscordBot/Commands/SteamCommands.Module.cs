using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamStatsDiscordBot.Model.JSON.GetAppList;
using SteamStatsDiscordBot.Model.JSON.GetIdByUserName;
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

        private JsonSerializer json = new JsonSerializer();

        //Among us app id = 945360
        [Command("concurrent_players")]
        [Aliases("c", "players_on")]
        [Description("Returns the current amount of active players.")]
        public async Task ConcurrentPlayers(CommandContext ctx, [RemainingText] string appid)
        {
            var msg = await getNumberOfConcurrentPlayers(appid.ToLower());
            await ctx.TriggerTypingAsync();
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
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var stringTask = client.GetStringAsync($"https://api.steampowered.com/ISteamApps/GetAppList/v2/");
            var response = JsonConvert.DeserializeObject<AppListRoot>(await stringTask);
            var appList = response.appList;
            var apps = appList.apps;
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
                return "Profile is set to private, or request has failed.";
            }
        }

        public async Task<string> getUserWishlist(string steamId)
        {
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
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
