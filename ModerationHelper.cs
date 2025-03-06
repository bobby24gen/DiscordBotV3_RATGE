using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
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

        public ModerationHelper(DiscordSocketClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            _client = client;

            _client.MessageDeleted += OnMessageDeleted;
        }

        public Task OnMessageDeleted(Cacheable<IMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2)
        {
            if (cacheable1.HasValue == false)
            {
                Console.WriteLine("A message was deleted, but its content could not be retrieved from cache.");
            }
            else
            {
                var orwell = (IMessageChannel)_client.GetChannel(1347165075378667590);
                orwell.SendMessageAsync($"Юзер {cacheable1.Value.Author} в канале {cacheable2.Value.Name} удалил сообщение: " +
                    $"{cacheable1.Value.Content}");

            }


            return Task.CompletedTask;
        }

    }
}
