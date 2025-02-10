namespace DiscordBotV3;


using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Rest.Entities.Tracks;

[DontAutoRegister]
public sealed class TTCModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IAudioService _audioService;
    private readonly DiscordSocketClient _client;

    public TTCModule(IAudioService audioService, DiscordSocketClient client)
    {
        _audioService = audioService;
        _client = client;
    }

}

