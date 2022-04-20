using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("добавить", "добавить пользователя в список рулетки с ссылкой на него", "@тег_пользователя")]
    public class HandlerAddParticipant : HandlerBase
    {
        public HandlerAddParticipant(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var data = message.Text?.Split(" ");

            var chatId = message.Chat.Id;

            if (data == null || data.Length < 2)
            {
                throw Error("Укажи пользователя, дурачок");
            }

            var from = message.From;
            if (from == null)
            {
                throw Error("Неизвестный пользователь");
            }

            var chatMember = await Client.GetChatMemberAsync(chatId, from.Id);

            if (chatMember.Status != ChatMemberStatus.Administrator && chatMember.Status != ChatMemberStatus.Creator)
            {
                throw Error("А тебе низя такое делать!");
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
                Error("Какой-то неправильный пользователь");
                return;
            }

            if (await RepositoryContainer.Participant.IsStartedForUser(username, chatId))
            {
                await SendTextAsync($"Этот парень итак в деле");
            }
            else
            {
                await RepositoryContainer.Participant.AddUser(username, chatId);

                await SendTextAsync($"Поздравляю, ты в деле, @{username}!");
            }
        }
    }
}