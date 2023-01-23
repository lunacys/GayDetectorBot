﻿using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling.Handlers
{
    [MessageHandler("пидордня", "стать участником рулетки", MemberStatusPermission.All)]
    public class HandlerGayOfTheDay : HandlerBase
    {
        private readonly IParticipantRepository _participantRepository;

        public HandlerGayOfTheDay(IParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;
            var from = message?.From;

            if (from == null || from.Username == null)
                return;

            if (await _participantRepository.IsStartedForUser(from.Username, chatId))
            {
                await SendTextAsync($"Ты итак в деле, @{from.Username}", message.MessageId);
            }
            else
            {
                await _participantRepository.AddUser(from.Username, chatId, from.FirstName, from.LastName);

                await SendTextAsync($"Поздравляю, ты в деле, @{from.Username}!", message.MessageId);
            }
        }
    }
}