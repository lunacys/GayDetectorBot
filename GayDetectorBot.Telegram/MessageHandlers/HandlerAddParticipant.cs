using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandlers
{
    [MessageHandler("добавить", "", 1)]
    public class HandlerAddParticipant : IMessageHandler
    {
        public string CommandString => "добавить";

        public bool HasParameters => true;

        public MemberStatusPermission Permissions =>
            MemberStatusPermission.Administrator | MemberStatusPermission.Creator;

        private readonly ParticipantRepository _participantRepository;

        public HandlerAddParticipant(ParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var data = message.Text.Split(" ");

            var chatId = message.Chat.Id;

            if (data.Length < 2)
            {
                await client.SendTextMessageAsync(chatId, "Укажи пользователя, дурачок");
                return;
            }

            var from = message.From;
            var chatMember = await client.GetChatMemberAsync(chatId, from.Id);

            if (chatMember.Status != ChatMemberStatus.Administrator && chatMember.Status != ChatMemberStatus.Creator)
            {
                await client.SendTextMessageAsync(chatId, "А тебе низя такое делать!");
                return;
            }

            var userRaw = data[1];
            if (string.IsNullOrEmpty(userRaw))
                return;

            string username;

            if (userRaw.StartsWith("@")) // Mention
            {
                username = userRaw.Replace("@", "");
            }
            else if (userRaw.StartsWith("\"") && userRaw.EndsWith("\""))
            {
                username = userRaw.Replace("\"", "");
            }
            else
            {
                await client.SendTextMessageAsync(chatId, "Какой-то неправильный пользователь");
                return;
            }

            if (await _participantRepository.IsStartedForUser(username, chatId))
            {
                await client.SendTextMessageAsync(chatId, $"Этот парень итак в деле");
            }
            else
            {
                await _participantRepository.AddUser(username, chatId);

                await client.SendTextMessageAsync(chatId, $"Поздравляю, ты в деле, @{username}!");
            }
        }
    }
}