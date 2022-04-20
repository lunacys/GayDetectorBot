using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("топпидоров", "узнать топ пидоров за всё время", MemberStatusPermission.All)]
    public class HandlerGayTop : HandlerBase
    {
        public HandlerGayTop(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;

            var gays = (await RepositoryContainer.Gay.RetrieveGays(chatId)).ToList();

            if (gays.Count == 0)
            {
                await SendTextAsync("Это удивительно, но пидоров на этом сервере нет");
            }
            else
            {
                var msg = $"**Топ пидоров за всё время:**\n";

                var data = gays.GroupBy(gay => gay.Participant.Username).Select(gr => gr.Key).ToList();

                var map = new Dictionary<string, (int, bool, string)>();

                foreach (var username in data)
                {
                    var count = gays.Count(gay => gay.Participant.Username == username);

                    map[username] = (count, gays.Find(gay => gay.Participant.Username == username)?.Participant?.IsRemoved ?? false, username);
                }

                var mapSorted = map.ToList();

                mapSorted.Sort((p1, p2) =>
                {
                    if (p1.Value.Item1 > p2.Value.Item1)
                        return -1;
                    if (p1.Value.Item1 < p2.Value.Item1)
                        return 1;
                    return 0;
                });

                for (int i = 0; i < mapSorted.Count; i++)
                {
                    //var lastTime

                    msg += $" > {i + 1}) @{mapSorted[i].Key} - {mapSorted[i].Value.Item1}";

                    if (mapSorted[i].Value.Item2)
                        msg += " - решил уйти от обязательств";

                    msg += "\n";
                }

                await SendTextAsync(msg, ParseMode.Markdown);
            }
        }
    }
}