using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Models.Requests;
using Starcraft.Stat.Services;

namespace Starcraft.Stat.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController(IGameService gameService, IStatisticsService statisticsService) : ControllerBase
{
    [HttpPost("Add")]
    public async Task<IActionResult> AddGame([FromQuery] bool pretty, [FromBody] AddGameRequest request)
    {
        try
        {
            await gameService.AddGameAsync(request);
            var response = await statisticsService.GetPlayerStatisticsAsync(request.ShowHistory, true);
            return Ok(pretty ? response.ToPretty() : response);
        }
        catch
        {
            return BadRequest();
        }
    }
}