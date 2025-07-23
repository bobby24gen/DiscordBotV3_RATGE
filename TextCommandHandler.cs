﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotV3
{
    public class TextCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        // Retrieve client and CommandService instance via ctor
        public TextCommandHandler(DiscordSocketClient client, 
            CommandService commands, 
            IServiceProvider serviceProvider, 
            IConfiguration configuration)
        {
            _commands = commands;
            _client = client;
            _services = serviceProvider;
            _configuration = configuration;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: _services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (message.Author.IsBot)
                return;
            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);
            
            await React(context);
            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }

        public async Task React(SocketCommandContext Context)
        {
            Random _random = new Random();
            var serverReactions = Context.Guild.Emotes.ToArray();

            int random = _random.Next(serverReactions.Length);
            Random rng = new Random();
            int rand = rng.Next(100);
            //Console.WriteLine($"{rand} выпал, он меньше 10? - {rand < 10}");
            if (rand < 10)
            {
                await Context.Message.AddReactionAsync(serverReactions[random]);
            }
        }
    }
}
