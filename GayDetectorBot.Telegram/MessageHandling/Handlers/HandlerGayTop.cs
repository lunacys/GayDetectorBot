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

                var map = new Dictionary<string, (int timesGay, bool isRemoved)>();

                foreach (var username in data)
                {
                    var count = gays.Count(gay => gay.Participant.Username == username);

                    map[username] = (count, gays.Find(gay => gay.Participant.Username == username)?.Participant?.IsRemoved ?? false);
                }

                var mapSorted = map.ToList();

                mapSorted.Sort((p1, p2) =>
                {
                    if (p1.Value.timesGay > p2.Value.timesGay)
                        return -1;
                    if (p1.Value.timesGay < p2.Value.timesGay)
                        return 1;
                    return 0;
                });

                for (int i = 0; i < mapSorted.Count; i++)
                {
                    //await SendTextAsync($" > {i + 1}) {mapSorted[i].Key} - {mapSorted[i].Value.timesGay}", ParseMode.Markdown);

                    //var lastTime
                    msg += $"> {i + 1}) {mapSorted[i].Key.Trim().Replace('_', ' ')} - {mapSorted[i].Value.timesGay}";
                    //msg += $" > {i + 1}) {mapSorted[i].Key} - {mapSorted[i].Value.timesGay}";

                    //if (mapSorted[i].Value.isRemoved)
                    //    msg += " - решил уйти от обязательств";

                    msg += "\n";
                }

                await SendTextAsync(msg, ParseMode.Markdown);
            }
        }
    }
}