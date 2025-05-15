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

/// <summary>
///     Presents some of the main features of the Lavalink4NET-Library.
/// </summary>
[RequireContext(ContextType.Guild)]
[RequireRole("Verified", Group = "DJ")]
[RequireOwner(Group = "DJ")]
public sealed class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IAudioService _audioService;
    private readonly DiscordSocketClient _client;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MusicModule"/> class.
    /// </summary>
    /// <param name="audioService">the audio service</param>
    /// <exception cref="ArgumentNullException">
    ///     thrown if the specified <paramref name="audioService"/> is <see langword="null"/>.
    /// </exception>
    public MusicModule(IAudioService audioService, DiscordSocketClient client)
    {
        ArgumentNullException.ThrowIfNull(audioService);

        _audioService = audioService;
        _client = client;

        _audioService.TrackEnded += _audioService_TrackEnded;
    }


    /// <summary>
    ///     Выходим из дискорд канала, если плейлист пуст после последнего трека
    /// </summary>
    /// <returns></returns>
    private Task _audioService_TrackEnded(object sender, Lavalink4NET.Events.Players.TrackEndedEventArgs eventArgs)
    {
        if (eventArgs.Player != null && eventArgs.Player.CurrentTrack == null)
        {
            eventArgs.Player.DisconnectAsync();
            return Task.CompletedTask;
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    ///     Disconnects from the current voice channel connected to asynchronously.
    /// </summary>
    /// <returns>a task that represents the asynchronous operation</returns>
    [SlashCommand("disconnect", "Отключиться от канала", runMode: RunMode.Async)]
    public async Task Disconnect()
    {
        var player = await GetPlayerAsync().ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        await player.DisconnectAsync().ConfigureAwait(false);
        await RespondAsync("Disconnected.",ephemeral:true).ConfigureAwait(false);
    }

    /// <summary>
    ///     Plays music asynchronously.
    /// </summary>
    /// <param name="query">the search query</param>
    /// <returns>a task that represents the asynchronous operation</returns>
    [SlashCommand("play", description: "Играем музыку, закидываем URL или текст для поиска в ютубе", runMode: RunMode.Async)]
    public async Task Play(string query)
    {
        await DeferAsync(true).ConfigureAwait(false);

        var player = await GetPlayerAsync(connectToVoiceChannel: true).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        var track = await _audioService.Tracks
            .LoadTrackAsync(query, TrackSearchMode.YouTube)
            .ConfigureAwait(false);

        if (track is null)
        {
            await FollowupAsync("😖 Нет результатов по запросу.", ephemeral: true).ConfigureAwait(false);
            await Disconnect();
            return;
        }

        var position = await player.PlayAsync(track).ConfigureAwait(false);

        if (position is 0)
        {
            await FollowupAsync($"🔈 Играем: {track.Uri}", ephemeral: true).ConfigureAwait(false);
        }
        else
        {
            await FollowupAsync($"🔈 Добавили в список: {track.Uri}", ephemeral: true).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Shows the track position asynchronously.
    /// </summary>
    /// <returns>a task that represents the asynchronous operation</returns>
    [SlashCommand("timeleft", description: "Показываем время трека", runMode: RunMode.Async)]
    public async Task Position()
    {
        var player = await GetPlayerAsync(connectToVoiceChannel: false).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentTrack is null)
        {
            await RespondAsync("Ничего не играет!", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await RespondAsync($"Позиция: {player.Position?.Position} / {player.CurrentTrack.Duration}.", ephemeral: true)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Stops the current track asynchronously.
    /// </summary>
    /// <returns>a task that represents the asynchronous operation</returns>
    [SlashCommand("stop", description: "Останавливает текущий трек", runMode: RunMode.Async)]
    public async Task Stop()
    {
        var player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentItem is null)
        {
            await RespondAsync("Ничего не играет!", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await player.StopAsync().ConfigureAwait(false);
        await RespondAsync("Остановился.",ephemeral: true).ConfigureAwait(false);
    }

    /// <summary>
    ///     Updates the player volume asynchronously.
    /// </summary>
    /// <param name="volume">the volume (1 - 1000)</param>
    /// <returns>a task that represents the asynchronous operation</returns>
    [SlashCommand("volume", description: "Устанавливает громкость звука (0 - 1000%)", runMode: RunMode.Async)]
    public async Task Volume(int volume = 100)
    {
        if (volume is > 1000 or < 0)
        {
            await RespondAsync("Чумба, ты ебанулся? Значения только: 0% - 1000%!", ephemeral: true).ConfigureAwait(false);
            return;
        }

        var player = await GetPlayerAsync(connectToVoiceChannel: false).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        await player.SetVolumeAsync(volume / 100f).ConfigureAwait(false);
        await RespondAsync($"Громкостьь изменёна: {volume}%", ephemeral: false).ConfigureAwait(false);
    }

    [SlashCommand("skip", description: "Скип трека", runMode: RunMode.Async)]
    public async Task Skip()
    {
        var player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentItem is null)
        {
            await RespondAsync("Ничего не играет!", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await player.SkipAsync().ConfigureAwait(false);

        var track = player.CurrentItem;

        if (track is not null)
        {
            await RespondAsync($"Скипнули, теперь играет: {track.Track!.Uri}", ephemeral: true).ConfigureAwait(false);
        }
        else
        {
            await RespondAsync("Скипнули, ничего нет в списке треков, останавливаемся.", ephemeral: true).ConfigureAwait(false);
            await Disconnect();
        }
    }

    [SlashCommand("pause", description: "Останавливает трек.", runMode: RunMode.Async)]
    public async Task PauseAsync()
    {
        var player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.State is PlayerState.Paused)
        {
            await RespondAsync("Чумба, мы уже на паузе.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await player.PauseAsync().ConfigureAwait(false);
        await RespondAsync("Остановились.", ephemeral: true).ConfigureAwait(false);
    }

    [SlashCommand("resume", description: "Продолжаем играть.", runMode: RunMode.Async)]
    public async Task ResumeAsync()
    {
        var player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.State is not PlayerState.Paused)
        {
            await RespondAsync("Чумба, мы и не останавливались.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await player.ResumeAsync().ConfigureAwait(false);
        await RespondAsync("Продолжаем.", ephemeral: true).ConfigureAwait(false);
    }

    /// <summary>
    ///     Gets the guild player asynchronously.
    /// </summary>
    /// <param name="connectToVoiceChannel">
    ///     a value indicating whether to connect to a voice channel
    /// </param>
    /// <returns>
    ///     a task that represents the asynchronous operation. The task result is the lavalink player.
    /// </returns>
    private async ValueTask<VoteLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true)
    {
        var retrieveOptions = new PlayerRetrieveOptions(
            ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

        var result = await _audioService.Players
            .RetrieveAsync(Context, playerFactory: PlayerFactory.Vote, retrieveOptions)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Status switch
            {
                PlayerRetrieveStatus.UserNotInVoiceChannel => "Ты не в войс-канале(переподключись если ты там).",
                PlayerRetrieveStatus.BotNotConnected => "Ты как сюда попал?.",
                _ => "Unknown error.",
            };

            await FollowupAsync(errorMessage).ConfigureAwait(false);
            return null;
        }
        await result.Player.SetVolumeAsync(50 / 100f); // fix
        return result.Player;
    }
}