using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Models.Requests;
using Starcraft.Stat.Services;

namespace Starcraft.Stat.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly IGameService _gameService;

    public GameController(IGameService gameService, IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
        _gameService = gameService;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> AddGame([FromQuery]bool pretty, [FromBody]AddGameRequest request)
    {
        try
        {
            await _gameService.AddGameAsync(request);
            var response = await _statisticsService.GetPlayerStatisticsAsync();
            return Ok(pretty ? response.ToPretty() : response);
        }
        catch
        {
            return BadRequest();
        }
    }


}