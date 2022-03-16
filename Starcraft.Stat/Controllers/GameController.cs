using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Models.Requests;
using Starcraft.Stat.Services;

namespace Starcraft.Stat.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IStatisticsService _statisticsService;

    public GameController(IGameService gameService, IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
        _gameService = gameService;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> AddGame([FromQuery] bool pretty, [FromBody] AddGameRequest request)
    {
        try
        {
            await _gameService.AddGameAsync(request);
            var response = await _statisticsService.GetPlayerStatisticsAsync(request.ShowHistory, true);
            return Ok(pretty ? response.ToPretty() : response);
        }
        catch
        {
            return BadRequest();
        }
    }
}