using GayDetectorBot.WebApi.Data.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Tg.Handlers.GayHandling
{
    [MessageHandler("добавить", "@тег_пользователя")]
    [MessageHandlerMetadata("добавить пользователя в список рулетки с ссылкой на него", CommandCategories.Gays)]
    public class HandlerAddParticipant : HandlerBase<string>
    {
        private readonly IParticipantRepository _participantRepository;

        public HandlerAddParticipant(IParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public override async Task HandleAsync(Message message, string? userRaw)
        {
            var chatId = message.Chat.Id;

            if (userRaw == null)
            {
                throw Error("Укажи пользователя, дурачок");
            }

            var from = message.From;
            if (from == null)
            {
                throw Error($"Неизвестный пользователь");
            }

            var chatMember = await Client.GetChatMember(chatId, from.Id);

            if (chatMember.Status != ChatMemberStatus.Administrator && chatMember.Status != ChatMemberStatus.Creator)
            {
                throw Error("А тебе низя такое делать!");
            }

            if (string.IsNullOrEmpty(userRaw))
            {
                throw Error("Укажи пользователя, дурачок");
            }

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
                throw Error($"Какой-то неправильный пользователь `{userRaw}`");
            }

            if (await _participantRepository.IsStartedForUser(username, chatId))
            {
                await SendTextAsync($"Этот парень итак в деле", message.MessageId);
            }
            else
            {
                await _participantRepository.AddUser(username, chatId, from.FirstName, from.LastName);

                await SendTextAsync($"Поздравляю, ты в деле, @{username}!", message.MessageId);
            }
        }
    }
}