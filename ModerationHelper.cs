using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotV3
{
    public sealed class ModerationHelper : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;

        public ModerationHelper(DiscordSocketClient client, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(client);

            _client = client;
            _configuration = config;

            Console.WriteLine(_configuration.GetValue<ulong>("moderationChannel"));

            _client.MessageDeleted += OnMessageDeleted;
            _client.MessageUpdated += _client_MessageUpdated;
        }

        private Task _client_MessageUpdated(Cacheable<IMessage, ulong> cacheable, SocketMessage edited, ISocketMessageChannel socketMessageChannel)
        {
            if (cacheable.HasValue == false)
            {
                Console.WriteLine("A message was edited, but its content could not be retrieved from cache.");
                return Task.CompletedTask;
            }
            if (cacheable.Value.Author.IsBot) 
            {
                return Task.CompletedTask;
            }
            
            var orwell = (IMessageChannel)_client.GetChannel(_configuration.GetValue<ulong>("moderationChannel"));
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Сообщение отредактированно");
            builder.ThumbnailUrl = cacheable.Value.Author.GetAvatarUrl();
            builder.AddField("Юзер", $"{cacheable.Value.Author}");
            builder.AddField("Канал", $"{socketMessageChannel.Name}");

            builder.AddField("Оригинальный текст", $" {cacheable.Value.Content}");

            builder.AddField("Изменённый текст", $"{edited.Content}");
            

            Embed message = builder.Build();

            orwell.SendMessageAsync(embed: message);

            return Task.CompletedTask;
        }

        public Task OnMessageDeleted(Cacheable<IMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2)
        {
            if (cacheable1.HasValue == false)
            {
                Console.WriteLine("A message was deleted, but its content could not be retrieved from cache.");
                return Task.CompletedTask;
            }
            else
            {
                if (cacheable1.Value.Author.IsBot)
                {
                    return Task.CompletedTask;
                }

                var orwell = (IMessageChannel)_client.GetChannel(_configuration.GetValue<ulong>("moderationChannel"));
                StringBuilder attachmentsLink = new StringBuilder();
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle("Сообщение удалено");
                builder.ThumbnailUrl = cacheable1.Value.Author.GetAvatarUrl();
                builder.AddField("Юзер", $"{cacheable1.Value.Author}");
                builder.AddField("Канал", $"{cacheable2.Value.Name}");

                if (cacheable1.Value.Content.Length > 0)
                {
                    builder.AddField("Текст сообщения", $" {cacheable1.Value.Content}");
                }

                if (cacheable1.Value.Attachments.Count > 0)
                {
                    foreach (var embed in cacheable1.Value.Attachments)
                    {
                        attachmentsLink.Append(embed.Url + "\n");
                    }
                    builder.AddField("Ссылки на вложения", $"{attachmentsLink}");
                }

                Embed message = builder.Build();

                orwell.SendMessageAsync(embed: message);
            }


            return Task.CompletedTask;
        }

    }
}
