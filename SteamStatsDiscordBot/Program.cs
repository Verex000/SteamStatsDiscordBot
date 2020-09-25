﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using SteamStatsDiscordBot.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteamStatsDiscordBot
{
    class Program
    {
        /* This is the cancellation token we'll use to end the bot if needed(used for most async stuff). */
        private CancellationTokenSource _cts { get; set; }

        /* We'll load the app config into this when we create it a little later. */
        private IConfigurationRoot _config;

        /* These are the discord library's main classes */
        private DiscordClient _discord;
        private CommandsNextModule _commands;
        private InteractivityModule _interactivity;

        static async Task Main(string[] args) => await new Program().InitBot(args);

        async Task InitBot(string[] args)
        {
            try
            {
                Console.WriteLine("[info] Welcome to my bot!");
                _cts = new CancellationTokenSource();

                // Load the config file(we'll create this shortly)
                Console.WriteLine("[info] Loading config file..");
                var appRoot = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
                //var appRoot = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.LastIndexOf("/bin"));
                //appRoot = appRoot.Substring(0, appRoot.LastIndexOf("/") + 1);
                //appRoot = appRoot.Substring(0, appRoot.LastIndexOf("/") + 1);

                _config = new ConfigurationBuilder()
                    .SetBasePath(appRoot)
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                    .Build();

                // Create the DSharpPlus client
                Console.WriteLine("[info] Creating discord client..");
                _discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = _config.GetValue<string>("discord:token"),
                    TokenType = TokenType.Bot,
                    UseInternalLogHandler = true,
                    LogLevel = LogLevel.Debug
                });

                _discord.MessageCreated += async e =>
                {
                    if (e.Message.Content.ToLower().StartsWith("ping"))
                        await e.Message.RespondAsync("pong!");
                };

                // Create the interactivity module(I'll show you how to use this later on)
                _interactivity = _discord.UseInteractivity(new InteractivityConfiguration()
                {
                    PaginationBehaviour = TimeoutBehaviour.Delete, // What to do when a pagination request times out
                    PaginationTimeout = TimeSpan.FromSeconds(30), // How long to wait before timing out
                    Timeout = TimeSpan.FromSeconds(30) // Default time to wait for interactive commands like waiting for a message or a reaction
                });

                // Build dependancies and then create the commands module.
                var deps = BuildDeps();
                _commands = _discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = _config.GetValue<string>("discord:CommandPrefix"), // Load the command prefix(what comes before the command, eg "!" or "/") from our config file
                    Dependencies = deps // Pass the dependancies
                });

                Console.WriteLine("[info] Loading command modules..");

                var type = typeof(IModule); // Get the type of our interface
                var types = AppDomain.CurrentDomain.GetAssemblies() // Get the assemblies associated with our project
                    .SelectMany(s => s.GetTypes()) // Get all the types
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterface); // Filter to find any type that can be assigned to an IModule

                var typeList = types as Type[] ?? types.ToArray(); // Convert to an array
                foreach (var t in typeList)
                    _commands.RegisterCommands(t); // Loop through the list and register each command module with CommandsNext

                Console.WriteLine($"[info] Loaded {typeList.Count()} modules.");

                RunAsync(args).Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        async Task RunAsync(string[] args)
        {
            // Connect to discord's service
            Console.WriteLine("Connecting..");
            await _discord.ConnectAsync();
            Console.WriteLine("Connected!");

            // Keep the bot running until the cancellation token requests we stop
            while (!_cts.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromMinutes(1));
        }

        /* 
            DSharpPlus has dependancy injection for commands, this builds a list of dependancies. 
            We can then access these in our command modules.
        */
        private DependencyCollection BuildDeps()
        {
            using var deps = new DependencyCollectionBuilder();

            deps.AddInstance(_interactivity) // Add interactivity
                .AddInstance(_cts) // Add the cancellation token
                .AddInstance(_config) // Add our config
                .AddInstance(_discord); // Add the discord client

            return deps.Build();
        }
    }
}
