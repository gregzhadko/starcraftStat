using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Services;
using Telegram.Bot.Types;

namespace Starcraft.Stat.Controllers;

public class WebhookController(IBotHandleService botHandleService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        await botHandleService.HandleAsync(update);
        return Ok();
    }
}