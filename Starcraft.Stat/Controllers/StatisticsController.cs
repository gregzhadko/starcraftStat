using Microsoft.AspNetCore.Mvc;
using Starcraft.Stat.Models.Responses;
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
    public Task<StatisticsResponse> GetPlayersStatistics()
    {
        return _service.GetPlayerStatisticsAsync();
    }
}