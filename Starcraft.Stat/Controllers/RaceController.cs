using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.DbModels;

namespace Starcraft.Stat.Controllers;

[ApiController]
[Route("[controller]")]
public class RaceController(StarcraftDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Race>>> GetRace() => await context.Races.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Race>> GetRace(string id)
    {
        var race = await context.Races.FindAsync(id);

        if (race == null)
        {
            return NotFound();
        }

        return race;
    }
}