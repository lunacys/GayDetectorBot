﻿using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling.Handlers.GayHandling
{
    [MessageHandler("уберименя", "убрать из списка рулетки - команда только для настоящих пидоров", MemberStatusPermission.All)]
    public class HandlerRemoveMe : HandlerBase
    {
        private readonly IParticipantRepository _participantRepository;

        public HandlerRemoveMe(IParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var username = message.From?.Username;

            if (username == null)
                return;

            await _participantRepository.RemoveUser(message.Chat.Id, username);

            if (message.From == null)
            {
                throw Error($"Неизвестный пользователь");
            }

            await SendTextAsync($"Ну ты и пидор, @{message.From.Username}. Убрал тебя.", message.MessageId);
        }
    }
}