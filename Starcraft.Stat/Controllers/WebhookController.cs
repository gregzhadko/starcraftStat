using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Services;
using Telegram.Bot.Types;

namespace Starcraft.Stat.Controllers;

public class WebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService, [FromBody] Update update)
    {
        await handleUpdateService.EchoAsync(update);
        return Ok();
    }
}