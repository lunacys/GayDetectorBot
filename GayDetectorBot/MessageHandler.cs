using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GayDetectorBot.Models;

namespace GayDetectorBot
{
    class PrefixContent
    {
        public string Prefix { get; set; }
        public string Content { get; set; }
    }

    public class MessageHandler
    {
        private readonly UserRepository _userRepository;

        private Dictionary<ulong, List<PrefixContent>> _customCommandMap;

        public MessageHandler(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        private async Task InitializeCustomCommands()
        {
            _customCommandMap = new Dictionary<ulong, List<PrefixContent>>();

            var cmds = (await _userRepository.RetrieveAllCommands()).ToList();

            foreach (var cmd in cmds)
            {
                if (_customCommandMap.ContainsKey(cmd.GuildId))
                {
                    _customCommandMap[cmd.GuildId].Add(new PrefixContent
                        { Prefix = cmd.CommandPrefix, Content = cmd.CommandContent });
                }
                else
                {
                    _customCommandMap[cmd.GuildId] = new List<PrefixContent>();
                }
            }
        }

        public async Task Message(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            if (ch == null)
                return;

            var g = ch?.Guild;

            if (_customCommandMap == null)
            {
                await InitializeCustomCommands();
            }

            var lower = message.Content.ToLower().TrimEnd();

            if (lower.StartsWith("!добавить-команду "))
            {
                await AddCommand(message);
            }
            else if (lower.StartsWith("!удалить-команду "))
            {
                await DeleteCommand(message);
            }
            else if (lower.StartsWith("!добавить "))
            {
                await AddUser(message);
            }
            else if (lower == "!ктопидор")
            {
                await WhoIsGayToday(message);
            }
            else if (lower == "!пидордня")
            {
                await Start(message);
            }
            else if (lower == "!топпидоров")
            {
                await TopGays(message);
            }
            else if (lower == "!помоги")
            {
                await Help(message);
            }
            else if (lower == "!уберименя")
            {
                await Stop(message);
            }
            else if (lower == "!участники")
            {
                await Participants(message);
            }
            else if (lower == "!special-operations")
            {
                await message.Channel.SendMessageAsync(
@"
FOX-HOUND
🔫🔁
🔫🔃🔀🔁
🔫🔁🔃🔀
🔫↪
🔫🔁🔀🔃
🔫↪🔁
🔫🔃🔃🔀🔄
 Revolver
 🔫↪🔄🔁
🔫🔃🔀
🔫↪🔀🔀🔄🔀
 Ocelot
 🔫🔃🔄🔁↪🔃
🔫🔁↪↪
🔫🔁🔁↪
 I've been waiting for you, Solid Snake 
🔫🔃🔃🔃
🔫🔁↪🔃🔄
🔫🔀🔁↪🔃🔃
 Now we'll see if the man can live up to the legend 
🔫🔀🔁🔀 
This is the greatest handgun ever made
🔫↘✋✊
The Single Action Army.
🔫⬆🔃🔃🔃🔃
Six bullets 
🔫⬆▫⬇▫⬇▫⬇▫⬇▫⬇▫⬇ 
More than enough to kill anything that moves
 🔫⬅▫⬅▫⬅▫⬅▫⬅▫⬅▫ 
🔫⬆🔃🔃🔃⬇🔃🔃🔃" +
                    "\nNow i'll show you why they call me \"ᏒᏋᏉᎧᏝᏉᏋᏒ\""
                );
            }
            else if (lower == "!поминутно")
            {
                await message.Channel.SendMessageAsync(
                    "Я распишу нахуй, поминутно блять, где кто соснул хуйца, блять. Кто сломал вещи, блять, купил амулет, афк встал, свинья ебаная. Через 2 недели блять ты умрешь от беспрерывного поноса блять. Я щас опять добавлю громкости блять, мразь ебаная блять, чтоб ты меня слышала сучка нахуй, поняла блять?");
            }

            if (_customCommandMap.ContainsKey(g.Id))
            {
                var content = _customCommandMap[g.Id].FirstOrDefault(pc => pc.Prefix == message.Content);
                if (content != null)
                    await message.Channel.SendMessageAsync(content.Content);
            }
        }

        private async Task AddUser(SocketMessage message)
        {
            var data = message.Content.Split(" ");
            if (data.Length < 2)
            {
                await message.Channel.SendMessageAsync("Укажи пользователя, дурачок");
                return;
            }

            if (message.Author.Id != 140479912506032128) // loonacuse#1111
            {
                await message.Channel.SendMessageAsync("А тебе низя такое делать!");
                return;
            }

            var userRaw = data[1];
            if (string.IsNullOrEmpty(userRaw))
                return;

            ulong userId;

            if (userRaw.StartsWith("<@")) // Mention
            {
                userId = MentionUtils.ParseUser(userRaw);
            }
            else if (char.IsDigit(userRaw[0])) // User Id
            {
                userId = ulong.Parse(userRaw);
            }
            else
            {
                await message.Channel.SendMessageAsync("Какой-то неправильный пользователь");
                return;
            }

            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            if (await _userRepository.IsStartedForUser(userId, g.Id))
            {
                await message.Channel.SendMessageAsync($"Этот парень итак в деле");
            }
            else
            {
                var user = await message.Channel.GetUserAsync(userId);

                await _userRepository.AddUser(user, g.Id);

                await message.Channel.SendMessageAsync($"Поздравляю, ты в деле, {user.Mention}!");
            }
        }

        private async Task WhoIsGayToday(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            var lastCheck = await _userRepository.GuildLastChecked(g.Id);

            var today = DateTime.Today;

            if (lastCheck.HasValue && lastCheck.Value.Day == today.Day && lastCheck.Value.Month == today.Month &&
                lastCheck.Value.Year == today.Year)
            {
                var gayToday = await _userRepository.GetLastGay(g.Id);

                if (gayToday.HasValue)
                {
                    var gayUser = await message.Channel.GetUserAsync(gayToday.Value);
                    await message.Channel.SendMessageAsync($"Сегодня пидор {gayUser.Mention}\n" +
                                                           $"Следующее обновление через {today}");
                    return;
                }
                else
                {
                    await message.Channel.SendMessageAsync($"Пидор не обнаружен");
                }
            }

            await message.Channel.SendMessageAsync("Выполняю поиск пидора...");

            var pList = (await _userRepository.RetrieveParticipants(g.Id)).Where(p => !p.IsRemoved).ToList();
            
            var rand = new Random();
            var i = rand.Next(pList.Count);

            var p = pList[i];

            if (!lastCheck.HasValue)
                await _userRepository.GuildAdd(g.Id, DateTimeOffset.Now, p.UserId);
            else
                await _userRepository.GuildUpdate(g.Id, p.UserId);
                                        
            await _userRepository.AddGay(p);

            await Task.Delay(1000);
            await message.Channel.SendMessageAsync("Пидор обнаружен");
            await Task.Delay(1000);

            var user = await message.Channel.GetUserAsync(p.UserId);

            await message.Channel.SendMessageAsync($"Сегодня ты пидор, {user.Mention}");
        }

        private async Task Start(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;
            var userId = message.Author.Id;

            if (await _userRepository.IsStartedForUser(userId, g.Id))
            {
                await message.Channel.SendMessageAsync($"Ты итак в деле");
            }
            else
            {
                var user = await message.Channel.GetUserAsync(userId);

                await _userRepository.AddUser(user, g.Id);

                await message.Channel.SendMessageAsync($"Поздравляю, ты в деле, {user.Mention}!");
            }
        }

        private async Task TopGays(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            var gays = (await _userRepository.RetrieveGays(g.Id)).ToList();

            if (gays.Count == 0)
            {
                await message.Channel.SendMessageAsync("Это удивительно, но пидоров на этом сервере нет");
            }
            else
            {
                var msg = $"**Топ пидоров за всё время:**";

                var data = gays.GroupBy(gay => gay.Participant.UserId).Select(gr => gr.Key).ToList();

                var map = new Dictionary<string, (int, bool)>();

                foreach (var userId in data)
                {
                    //var user = await message.Channel.GetUserAsync(userId);
                    var u2 = g.GetUser(userId).Nickname;

                    var count = gays.Count(gay => gay.Participant.UserId == userId);

                    map[u2] = (count, gays.Find(gay => gay.Participant.UserId == userId)?.Participant?.IsRemoved ?? false);
                }

                var mapSorted = map.ToList();

                mapSorted.Sort((p1, p2) =>
                {
                    if (p1.Value.Item1 > p2.Value.Item1)
                        return 1;
                    if (p1.Value.Item1 < p2.Value.Item1)
                        return -1;
                    return 0;
                });

                for (int i = 0; i < mapSorted.Count; i++)
                {
                    msg += $" > {i + 1}) {mapSorted[i].Key} - {mapSorted[i].Value}";

                    if (mapSorted[i].Value.Item2)
                        msg += " - решил уйти от обязательств";

                    msg += "\n";
                }

                await message.Channel.SendMessageAsync(msg);
            }
        }

        private async Task Stop(SocketMessage message)
        {
            var userId = message.Author.Id; 
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            await _userRepository.RemoveUser(g.Id, userId);

            await message.Channel.SendMessageAsync($"Ну ты и пидор, {message.Author.Mention}. Убрал тебя.");
        }

        private async Task Help(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("Тебе уже ничто не поможет...\n\n" +
                                      "`!добавить <mention>` ИЛИ `!добавить <userid>` - добавить пользователя в список рулетки с ссылкой на него\n" +
                                      "`!ктопидор` - узнать пидора дня\n" +
                                      "`!топпидоров` - узнать топ пидоров за всё время\n" +
                                      "`!помоги` - увидеть это сообщение ещё раз\n" +
                                      "`!уберименя` - убрать из списка рулетки - команда только для настоящих пидоров\n" +
                                      "`!добавить-команду !<название-команды> <текстовое содержание>` - добавить кастомную команду\n" +
                                      "`!удалить-команду !<название-команды>` - удалить кастомную команду");
        }

        private async Task AddCommand(SocketMessage message)
        {
            var data = message.Content.Split(' ');
            
            if (data.Length < 3)
            {
                await message.Channel.SendMessageAsync("Мало данных! Надо два параметра!");
                return;
            }

            var prefix = data[1];

            if (!prefix.StartsWith('!'))
            {
                await message.Channel.SendMessageAsync("Команды должны начинаться со знака `!`");
                return;
            }

            var content = data[2];

            for (int i = 3; i < data.Length; i++)
            {
                content += " " + data[i];
            }

            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            if (await _userRepository.CommandExists(prefix, g.Id))
            {
                await message.Channel.SendMessageAsync($"Команда `{prefix}` уже существует!");
            }
            else
            {
                await _userRepository.AddCommand(g.Id, message.Author.Id, prefix, content);

                if (!_customCommandMap.ContainsKey(g.Id))
                {
                    _customCommandMap[g.Id] = new List<PrefixContent>();
                }

                _customCommandMap[g.Id].Add(new PrefixContent
                {
                    Prefix = prefix,
                    Content = content
                });

                await message.Channel.SendMessageAsync($"Команда `{prefix}` добавлена успешно");
            }
        }

        private async Task DeleteCommand(SocketMessage message)
        {
            var data = message.Content.Split(' ');

            if (data.Length < 2)
            {
                await message.Channel.SendMessageAsync("Мало данных! Нужен один параметр!");
                return;
            }

            var prefix = data[1];

            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            if (!await _userRepository.CommandExists(prefix, g.Id))
            {
                await message.Channel.SendMessageAsync($"Команды `{prefix}` не существует");
                return;
            }

            await _userRepository.DeleteCommand(prefix, g.Id);
            _customCommandMap[g.Id]?.RemoveAll(pc => pc.Prefix == prefix);

            await message.Channel.SendMessageAsync($"Команда `{prefix}` успешно удалена");
        }

        private async Task Participants(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            var pList = await _userRepository.RetrieveParticipants(g.Id);

            string listStr = "";

            foreach (var p in pList)
            {
                var u = await message.Channel.GetUserAsync(p.UserId);
                listStr += $" - {u.Mention}\n";
            }

            await message.Channel.SendMessageAsync("Участники:\n\n" + listStr);
        }
    }
}