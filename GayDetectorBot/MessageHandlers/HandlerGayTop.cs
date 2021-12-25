using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerGayTop : IMessageHandler
    {
        public string CommandString => "!топпидоров";

        public bool HasParameters => false;

        private readonly GayRepository _gayRepository;

        public HandlerGayTop(GayRepository gayRepository)
        {
            _gayRepository = gayRepository;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            var gays = (await _gayRepository.RetrieveGays(g.Id)).ToList();

            if (gays.Count == 0)
            {
                await message.Channel.SendMessageAsync("Это удивительно, но пидоров на этом сервере нет");
            }
            else
            {
                var msg = $"**Топ пидоров за всё время:**\n";

                var data = gays.GroupBy(gay => gay.Participant.UserId).Select(gr => gr.Key).ToList();

                var map = new Dictionary<string, (int, bool, ulong)>();

                foreach (var userId in data)
                {
                    //var user = await message.Channel.GetUserAsync(userId);
                    var u2 = g.GetUser(userId).Nickname;

                    var count = gays.Count(gay => gay.Participant.UserId == userId);

                    map[u2] = (count, gays.Find(gay => gay.Participant.UserId == userId)?.Participant?.IsRemoved ?? false, userId);
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
                    //var lastTime

                    msg += $" > {i + 1}) {mapSorted[i].Key} - {mapSorted[i].Value.Item1}";

                    if (mapSorted[i].Value.Item2)
                        msg += " - решил уйти от обязательств";

                    msg += "\n";
                }

                await message.Channel.SendMessageAsync(msg);
            }
        }
    }
}