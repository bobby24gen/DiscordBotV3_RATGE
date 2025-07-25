namespace DiscordBotV3;

using AngleSharp;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using static DiscordBotV3.JsonClassHelper;
using static System.Formats.Asn1.AsnWriter;

[RequireContext(ContextType.Guild)]
public sealed class CommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;

    public Random random = new Random();

    public CommandsModule( DiscordSocketClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;

        _client.ButtonExecuted += _client_ButtonExecuted;
    }

    public async Task _client_ButtonExecuted(SocketMessageComponent arg)
    {

        switch (arg.Data.CustomId)
        {
            case "addpidoras":
                List<string> listOfpidorasi = new SqliteHelper().GetPidorasi().ConvertAll(x => x.ToLower());
                SqliteHelper sqliteHelper = new SqliteHelper();

                if (listOfpidorasi.FirstOrDefault(x => x.Contains(arg.User.Mention.ToLower())) != null)
                {
                    await arg.RespondAsync($"В списке пидорасов ты уже есть", ephemeral: true);
                    break;
                }

                sqliteHelper.AddPidorasi(arg.User.Mention);
                
                await arg.UpdateAsync(x =>
                {
                    x.Content = $"{arg.User.Mention} добавился в список пидорасов, поздравим его";
                });
                break;
            default:
                break;
        }
    }

    public List<string> coolRoles = new List<string>()
    {
        "Крысюк",
        "Крутышки"
    };

    [SlashCommand("fantic","крыса")]
    public async Task Fantic()
    {
        await DeferAsync(false).ConfigureAwait(false);

        await FollowupAsync("https://tenor.com/view/rat-spin-gif-10300642414513246571\r\n");
    }


    [SlashCommand("roll", "ролим, даём макс значения")]    
    public async Task Roll(
        [Summary("Max","Максимальное значение ролла")]
        int Max,
        [Summary("Min","Минимальное значение ролла")]
        int? Min = null,
        [Summary("Reason","На что ролим?")]
        string? reason = "по приколу.")
    {
        await DeferAsync(false).ConfigureAwait(false);


        static Embed rollEmbedBuilder(int Max, int? Min, string? reason)
        {
            Random rnd = new Random();

            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle($"Выпало: {rnd.Next((int)Min, Max)}!");
            builder.AddField($"В промежутке от {Min} до {Max}", $"Причина: {reason}");
            builder.ThumbnailUrl = "https://media.discordapp.net/attachments/1051550324181180446/1146720897261109248/uvyDice.gif?ex=67f77f0e&is=67f62d8e&hm=4d3740216da5b6298e50913b9ed795ca942396430755188981cf816c3129cef9&";

            return builder.Build();
        }

        if (Min > Max)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Выпало: Error!");
            builder.AddField("В промежутке от nil до null", "Причина: минимум не может быть больше максимума!");
            builder.ThumbnailUrl = "https://tenor.com/view/cat-catcry-gif-19131995";

            Embed embed = builder.Build();

            await FollowupAsync(embed: embed).ConfigureAwait(false);
        }
        if (Min != null)
        {
            Embed embed = rollEmbedBuilder(Max, Min, reason);

            await FollowupAsync(embed: embed).ConfigureAwait(false);
        }
        else if (Max < 0)
        {
            Embed embed = rollEmbedBuilder(0, Max, reason);

            await FollowupAsync(embed: embed).ConfigureAwait(false);
        }
        else
        {
            Embed embed = rollEmbedBuilder(Max, 0, reason);

            await FollowupAsync(embed: embed).ConfigureAwait(false);
        }
        
    }

    [SlashCommand("pidorasi", "список пидорасов")]
    public async Task Hate(
        [Summary("name","Добавляет имя в список")]
        string? name = null
        )
    {        
        
        await DeferAsync(false).ConfigureAwait(false);
       
        
        SqliteHelper sqliteHelper = new SqliteHelper();

        List<string> listOfpidorasi = new SqliteHelper().GetPidorasi();

        if (name == null)
        {
            var seppuku = new ComponentBuilder()
                    .WithButton("Добавить себя", "addpidoras");

            var messsageComp = seppuku.Build();

            string pidorasi = "";
            for (int i = 0; i < listOfpidorasi.Count; i++)
            {
                pidorasi += i.ToString() + ". " + listOfpidorasi[i] + "\n";
            }
            await FollowupAsync($"Список пидорасов:\n" +
                pidorasi +
                "Список активно дополняется, ВЫ можете дополнить его",allowedMentions:AllowedMentions.None,components:messsageComp);
        }
        else
        {
            List<string> listPidorasiLower = listOfpidorasi.ConvertAll(x => x.ToLower());
            
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }

            // Проверка на роли у пользователя, который вызвал комманду
            if (user.Roles.Any(r => r.Name == "Крысюк" || r.Name == "Крутышки"))
            {
                // Проверка на дубликат
                if (listPidorasiLower.FirstOrDefault(x => x.Contains(name.ToLower())) != null)
                {
                    await FollowupAsync($"В списке пидорасов, {name} уже присутствует <:ratgelupa:1276532532896202852>");
                    return;
                }

                sqliteHelper.AddPidorasi(name);
                await FollowupAsync($"Список пидорасов пополнился {name}");
            }else
            {
                var seppuku = new ComponentBuilder()
                    .WithButton("Добавить себя", "addpidoras");
                
                var messsageComp = seppuku.Build();

                await FollowupAsync("Нет прав <:gagaga:1276389397927039038>, хотите добавить себя? <:ratgelupa:1276532532896202852>", components:messsageComp);
            }
        }

    }    


    [SlashCommand("meme", "Смотрим мемы")]
    public async Task Meme(
        [Summary("tag","Смотрим рандомный мем по определённому тегу")]
        string tag       
        )
    {
        await DeferAsync(false).ConfigureAwait(false);
        SqliteHelper sqliteHelper = new SqliteHelper();

        string link = sqliteHelper.GetMemes(tag);
       
        if (link == "")
        {
            await FollowupAsync("Нет такого тега, посмотри теги по gettags");
        }
        
        await FollowupAsync(link);
    }

    [SlashCommand("addmeme", "Добавляем мемы")]
    [RequireOwner(Group = "CoolDudes")]
    [RequireRole("Крутышки", Group = "CoolDudes")]
    [RequireRole("Крысюк", Group = "CoolDudes")]
    public async Task Meme(
        [Summary("tag","Добавляем мем с определённым тегом")]
        string tag,
        [Summary("link","Добавляем ссылку на мем в список, tag - куда добавится мем")]
        string link
        )
    {
        await DeferAsync(false).ConfigureAwait(false);
        SqliteHelper sqliteHelper = new SqliteHelper();

        if (sqliteHelper.AddMeme(tag, link))
        {
            await FollowupAsync("Мем успешно добавлен");
        }
        else
        {
            await FollowupAsync("Произошла ошибка");
        }
    }


    [SlashCommand("gettags", "Получить список тегов для мемов")]
    public async Task GetTags()
    {
        await DeferAsync(false).ConfigureAwait(false);
        SqliteHelper sqliteHelper = new SqliteHelper();

        string tags = "";
        List<string> listOfTags = new SqliteHelper().GetTags();
        List<string> listOfTagsFiltered = listOfTags.Distinct().ToList();
        foreach (string pid in listOfTagsFiltered)
        {
            tags += pid + "\n";
        }

        await FollowupAsync($"Список тегов в мемах:\n" + tags);


    }

    [SlashCommand("talk", "silence, Miha is talking")]
    [RequireOwner]
    public async Task Talk(
        [Summary("text","silence, Miha is talking")]
        string text)
    {
        await DeferAsync(false).ConfigureAwait(false);

        await FollowupAsync(text, allowedMentions: AllowedMentions.All).ConfigureAwait(false);
        
    }

    [SlashCommand("hanadiscus", "Тебе сколько лет?")]
    public async Task hanadiscus()
    {
        await DeferAsync(false).ConfigureAwait(false);

        await FollowupAsync("Тебе сколько лет вообще?").ConfigureAwait(false);
    }

    [SlashCommand("emote", "Смотрим что за эмоут тут у нас")]
    public async Task GetEmoteData(
        [Summary("Emote","Даём эмоут на вход")]
        string text)
    {
        await DeferAsync(false).ConfigureAwait(false);
        Emote emote;
        try
        {
            
            if (IsAllDigits(text))
            {
                emote = Emote.Parse($"<:noname:{text}>");
            }
            else
            {
                emote = Emote.Parse(text);
            }
        }
        catch (Exception)
        {
            await FollowupAsync("Не получилось распарсить эмоут. Нужнен либо id, либо выбери его при наборе текста")
                .ConfigureAwait(false);
            throw;
        }
        
        
        EmbedBuilder builder = new EmbedBuilder();

        builder.WithTitle("Emote");
        builder.ThumbnailUrl = emote.Url;
        builder.AddField("Имя",$"{emote.Name}");
        builder.AddField("ID", $"{emote.Id}");
        builder.AddField("Анимированный", $"{emote.Animated}");
        builder.AddField("Ссылка", $"{emote.Url}");

        Embed embed = builder.Build();

        await FollowupAsync(embed: embed).ConfigureAwait(false);
    }

    //[SlashCommand("test", "тестим")]
    //[RequireOwner]
    //public async Task Test()
    //{
    //}

    [SlashCommand("img34bytag", "Случайный пост Gelbooru")]
    public async Task Img34ByTag(
        [Summary("tags", "Тэги через запятую")] string tags = "",
        [Summary("rate", "Рейтинг 1(general) 2(ecchi) 3(sex)")] int rate = 1)
    {
        await DeferAsync(false).ConfigureAwait(false);

        string baseUrl = "https://gelbooru.com/index.php?";
        string apiKey = "e3055e873170fef1e69e409fd1d48f832b01bb9097c19122e9b718112efa8c6bbdcc9410f501cff2f6922c1370d559447718fcf475ae218873d4eaec2102aeff";
        string userId = "1765730";
        string banTags = "+-video+-yaoi+-loli+-shota+-rape+-futa+-peeing+-fart+-diaper+-male_penetrated";
        string addTags = "";
        string rateLong = "general";

        while (tags[..1] == ",")
            tags = tags[1..];
        while (tags[tags.Length - 1] == ',')
            tags = tags[..^1];

        if (tags != "")
            addTags = '+' + tags.Replace(", ", "+").Replace(' ', '_');

        if (rate == 3)
            rateLong = "explicit";
        else if (rate == 2)
            rateLong = "questionable+-rating%3aSensitive";
        else if (rate == 1)
            rateLong = "general";

        string url = $"{baseUrl}page=dapi&s=post&q=index&tags=sort%3arandom{banTags}{addTags}+rating%3a{rateLong}+score%3a>{30}&limit=3&api_key={apiKey}&user_id={userId}&json=1";

        GelBooru? deserializeJson = null;
        string responseContent;

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);

            responseContent = await response.Content.ReadAsStringAsync();
        }

        if (responseContent != null)
        {
            deserializeJson = JsonConvert.DeserializeObject<GelBooru>(responseContent);
        }

        if (deserializeJson == null)
        {
            await FollowupAsync("ERROR ALL").ConfigureAwait(false);
            return;
        }

        if (deserializeJson.Post == null || deserializeJson.Post.Count != 3)
        {
            await FollowupAsync("ERROR TAGS").ConfigureAwait(false);
            return;
        }

        EmbedBuilder builder = new EmbedBuilder();

        builder.WithTitle($"id#{deserializeJson.Post[0].Id}#{deserializeJson.Post[1].Id}#{deserializeJson.Post[2].Id}");
        builder.Url = "https://twitter.com";
        if (deserializeJson.Post[0].SampleUrl != null)
            builder.ImageUrl = deserializeJson.Post[0].SampleUrl.ToString();
        else if (deserializeJson.Post[0].FileUrl != null)
            builder.ImageUrl = deserializeJson.Post[0].FileUrl.ToString();

        Embed embed = builder.Build();


        builder = new EmbedBuilder();

        builder.Url = "https://twitter.com";
        if (deserializeJson.Post[1].SampleUrl != null)
            builder.ImageUrl = deserializeJson.Post[1].SampleUrl.ToString();
        else if (deserializeJson.Post[1].FileUrl != null)
            builder.ImageUrl = deserializeJson.Post[1].FileUrl.ToString();

        Embed embed2 = builder.Build();


        builder = new EmbedBuilder();

        builder.Url = "https://twitter.com";
        if (deserializeJson.Post[2].SampleUrl != null) //Иногда пост не содержит этой ссылки
            builder.ImageUrl = deserializeJson.Post[2].SampleUrl.ToString();
        else if (deserializeJson.Post[2].FileUrl != null) //Более тяжелый файл
            builder.ImageUrl = deserializeJson.Post[2].FileUrl.ToString();

        Embed embed3 = builder.Build();

        await FollowupAsync(embeds: new[] { embed, embed2, embed3 }).ConfigureAwait(false);

        //EmbedBuilder builder = new EmbedBuilder();

        //builder.WithTitle($"Random (id#{deserializeJson.Post.First().Id})");
        //if (deserializeJson.Post.First().SampleUrl != null)
        //    builder.ImageUrl = deserializeJson.Post.First().SampleUrl.ToString();
        //else if (deserializeJson.Post.First().FileUrl != null)
        //    builder.ImageUrl = deserializeJson.Post.First().FileUrl.ToString();

        //Embed embed = builder.Build();

        //await FollowupAsync(embed: embed).ConfigureAwait(false);
    }

    [SlashCommand("img34byid", "Поиск поста Gelbooru по id")]
    public async Task Img34ById(
        [Summary("id","id изображения")] int id,
        [Summary("viewTags","Отображение тегов 0(нет) 1(да)")] int viewTags = 1)
    {
        await DeferAsync(false).ConfigureAwait(false);


        string url = $"https://gelbooru.com/index.php?page=post&s=view&id={id}";

        string img = "";
        string artist = "";
        string character = "";
        string copyright = "";
        string general = "";

        string htmlContent;

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);

            htmlContent = await response.Content.ReadAsStringAsync();
        }

        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        var document = await context.OpenAsync(req => req.Content(htmlContent));


        var imgDoc = document.QuerySelector(".image-container");

        if (imgDoc != null)
        {
            string image = imgDoc.QuerySelector("img")?.GetAttribute("src") ?? "Not available";

            img = image;
        }


        var artistDoc = document.QuerySelectorAll(".tag-type-artist");

        if (artistDoc != null)
        {

            foreach (var item in artistDoc)
            {
                string name = "";

                var items = item.QuerySelectorAll("a");

                if (items != null)
                {
                    foreach (var item2 in items)
                    {
                        name += item2.TextContent;
                    }
                }
                artist += name[1..] + ", ";
            }
        }


        var characterDoc = document.QuerySelectorAll(".tag-type-character");

        if (characterDoc != null)
        {

            foreach (var item in characterDoc)
            {
                string name = "";

                var items = item.QuerySelectorAll("a");

                if (items != null)
                {
                    foreach (var item2 in items)
                    {
                        name += item2.TextContent;
                    }
                }
                character += name[1..] + ", ";
            }
        }


        var copyrightDoc = document.QuerySelectorAll(".tag-type-copyright");

        if (copyrightDoc != null)
        {

            foreach (var item in copyrightDoc)
            {
                string name = "";

                var items = item.QuerySelectorAll("a");

                if (items != null)
                {
                    foreach (var item2 in items)
                    {
                        name += item2.TextContent;
                    }
                }
                copyright += name[1..] + ", ";
            }
        }


        var generalDoc = document.QuerySelectorAll(".tag-type-general");

        if (generalDoc != null)
        {

            foreach (var item in generalDoc)
            {
                string name = "";

                var items = item.QuerySelectorAll("a");

                if (items != null)
                {
                    foreach (var item2 in items)
                    {
                        name += item2.TextContent;
                    }
                }
                general += name[1..] + ", ";
            }
        }


        EmbedBuilder builder = new EmbedBuilder();

        builder.WithTitle($"Search by id#{id}");
        builder.Url = url;
        if (img != "")
            builder.ImageUrl = img;
        if (viewTags == 1)
        {
            if (artist != "")
                builder.AddField("Artist", $"{artist[..^2]}");
            if (character != "")
                builder.AddField("Character", $"{character[..^2]}");
            if (copyright != "")
                builder.AddField("Copyright", $"{copyright[..^2]}");
            if (general != "")
                builder.AddField("General", $"{general[..^2]}");
        }
        Embed embed = builder.Build();

        await FollowupAsync(embed: embed).ConfigureAwait(false);
    }

    [SlashCommand("deletetag", "Удаляем мемы с определённым тегом")]
    [RequireOwner(Group = "CoolDudes")]
    [RequireRole("Крысюк", Group = "CoolDudes")]
    public async Task DelTag(
        [Summary("Tag","Тег который нужно удалить полностью")]
        string tag
        )
    {
        await DeferAsync(false).ConfigureAwait(false);

        SqliteHelper sqliteHelper = new SqliteHelper();

        if (sqliteHelper.DelTag(tag))
        {
            await FollowupAsync($"Мемы с тегом {tag} удалены");
        }
        else
        {
            await FollowupAsync("Такого тега и так нет");
        }
    }

    [SlashCommand("deletememe", "Удаляем мем с определённой ссылкой")]
    [RequireOwner(Group = "CoolDudes")]
    [RequireRole("Крысюк", Group = "CoolDudes")]
    public async Task DelMeme(
        [Summary("Link","Ссылка на мем, который нужно удалить")]
        string link
        )
    {
        await DeferAsync(false).ConfigureAwait(false);

        SqliteHelper sqliteHelper = new SqliteHelper();

        if (sqliteHelper.DelMeme(link))
        {
            await FollowupAsync($"Мем {link} удалён");
        }
        else
        {
            await FollowupAsync("Такой ссылки нет");
        }
    }

    

    /*
    [SlashCommand("online", "Смотрим онлайн")]
    public async Task Online()       
    {
        await DeferAsync(false).ConfigureAwait(false);

        ByondHelper byondHelper = new ByondHelper();

        if (byondHelper.IsServerOnline())
        {
            await FollowupAsync(byondHelper.QueryServerStatus());
        }
        else
        {
            await FollowupAsync("Сервер мёртв ");
        }
    }*/

    public bool CheckPrivelege(string roleName)
    {
        var match = coolRoles
            .FirstOrDefault(coolRoles => coolRoles.Contains(roleName));
        if (match != null)
        {
            return true;
        }
        return false;
    }
    public static bool IsAllDigits(string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
    }
}
