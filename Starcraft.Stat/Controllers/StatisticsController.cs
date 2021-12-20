using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Services;

namespace Starcraft.Stat.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _service;

    public StatisticsController(IStatisticsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlayersStatistics(bool pretty = false)
    {
        var response = await _service.GetPlayerStatisticsAsync();
        return Ok(pretty ? response.ToPretty() : response);
    }
}