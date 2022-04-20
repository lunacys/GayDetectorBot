﻿using GayDetectorBot.Telegram.Data.Repos;
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

        public override async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var data = message.Text?.Split(" ");

            var chatId = message.Chat.Id;

            if (data == null || data.Length < 2)
            {
                await client.SendTextMessageAsync(chatId, "Укажи пользователя, дурачок");
                return;
            }

            var from = message.From;
            if (from == null)
            {
                await client.SendTextMessageAsync(chatId, "Неизвестный пользователь");
                return;
            }

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

            if (await RepositoryContainer.Participant.IsStartedForUser(username, chatId))
            {
                await client.SendTextMessageAsync(chatId, $"Этот парень итак в деле");
            }
            else
            {
                await RepositoryContainer.Participant.AddUser(username, chatId);

                await client.SendTextMessageAsync(chatId, $"Поздравляю, ты в деле, @{username}!");
            }
        }
    }
}