using GayDetectorBot.WebApi.Services.Tg;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Controllers;

public class BotController : ControllerBase
{
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] ITelegramService telegramService,
        CancellationToken cancellationToken)
    {
        await telegramService.HandleUpdateFromController(update, cancellationToken);
        return Ok();
    }
}