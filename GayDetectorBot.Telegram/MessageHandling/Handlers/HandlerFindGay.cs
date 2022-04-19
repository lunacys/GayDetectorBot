using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("ктопидор", "узнать пидора дня", MemberStatusPermission.All)]
    public class HandlerFindGay : IMessageHandler
    {
        public string CommandString => "!ктопидор";

        public MemberStatusPermission Permissions =>
            MemberStatusPermission.Administrator | MemberStatusPermission.Creator;

        public bool HasParameters => false;

        private readonly ChatRepository _chatRepository;
        private readonly ParticipantRepository _participantRepository;
        private readonly GayRepository _gayRepository;

        public HandlerFindGay(ChatRepository chatRepository, ParticipantRepository participantRepository, GayRepository gayRepository)
        {
            _chatRepository = chatRepository;
            _participantRepository = participantRepository;
            _gayRepository = gayRepository;
        }

        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;

            var lastCheck = await _chatRepository.ChatLastChecked(chatId);

            var today = DateTimeOffset.Now;

            if (lastCheck.HasValue && lastCheck.Value.Day == today.Day && lastCheck.Value.Month == today.Month &&
                lastCheck.Value.Year == today.Year)
            {
                var gayToday = await _chatRepository.GetLastGay(chatId);

                if (!string.IsNullOrEmpty(gayToday))
                {
                    var nextDate = today.Date.AddDays(1);

                    var delta = nextDate - today;

                    await client.SendTextMessageAsync(chatId, $"Сегодня пидор @{gayToday}\n" +
                                                              $"Следующее обновление через {delta.Hours}:{delta.Minutes}");
                    return;
                }
                else
                {
                    await client.SendTextMessageAsync(chatId, $"Пидор не обнаружен");
                }
            }

            await client.SendTextMessageAsync(chatId, "Выполняю поиск пидора...");

            var pList = (await _participantRepository.RetrieveParticipants(chatId)).Where(p => !p.IsRemoved).ToList();

            if (pList.Count == 0)
            {
                await client.SendTextMessageAsync(chatId, "Участников рулетки нет...");
                return;
            }

            var rand = new Random();
            var i = rand.Next(pList.Count);

            var p = pList[i];

            if (!lastCheck.HasValue)
                await _chatRepository.ChatAdd(chatId, DateTimeOffset.Now, p.Username);
            else
                await _chatRepository.ChatUpdate(chatId, p.Username);

            await _gayRepository.AddGay(p);

            await Task.Delay(1000);
            await client.SendTextMessageAsync(chatId, "Пидор обнаружен");
            await Task.Delay(1000);

            await client.SendTextMessageAsync(chatId, $"Сегодня ты пидор, @{p.Username}");
        }
    }
}