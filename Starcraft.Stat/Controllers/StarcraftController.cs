using Microsoft.AspNetCore.Mvc;

namespace Starcraft.Stat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StarcraftController : ControllerBase
{
    private readonly ILogger<StarcraftController> _logger;

    public StarcraftController(ILogger<StarcraftController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public string Get()
    {
        const string result = "Hello Starcraft";
        _logger.LogInformation(result);
        return result;
    }
}