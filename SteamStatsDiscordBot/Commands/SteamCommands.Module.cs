using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;
using SteamStatsDiscordBot.Model.JSON.GetAppList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SteamStatsDiscordBot.Commands
{
    class SteamCommands : IModule
    {
        private static readonly HttpClient client = new HttpClient();

        private JsonSerializer json = new JsonSerializer();

        //Among us app id = 945360
        [Command("concurrent_players")]
        public async Task ConcurrentPlayers(CommandContext ctx, [RemainingText] string appid)
        {
            var msg = await getNumberOfConcurrentPlayers(appid.ToLower());
            await ctx.RespondAsync($"{ctx.User.Mention} {msg}");
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
            var one = apps.FirstOrDefault<App>(app => app.name.ToLower() == appName);
            if (one == null)
            {
                return null;
            }
            return one.appId.ToString();
        }
    }
}
