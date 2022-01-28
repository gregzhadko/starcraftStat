using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Services;
using Telegram.Bot.Types;

namespace Starcraft.Stat.Controllers;

public class WebhookController : ControllerBase
{
    private readonly IBotHandleService _botHandleService;

    public WebhookController(IBotHandleService botHandleService)
    {
        _botHandleService = botHandleService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        await _botHandleService.HandleAsync(update);
        return Ok();
    }
}