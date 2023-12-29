using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Services;

namespace Starcraft.Stat.Controllers;

[ApiController]
[Route("[controller]")]
public class StatisticsController(IStatisticsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPlayersStatistics(bool pretty = false, bool showHistory = false)
    {
        var response = await service.GetPlayerStatisticsAsync(showHistory, true);
        return Ok(pretty ? response.ToPretty() : response);
    }
}