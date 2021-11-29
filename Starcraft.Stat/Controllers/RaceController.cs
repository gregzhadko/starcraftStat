using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.DbModels;

namespace Starcraft.Stat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaceController : ControllerBase
    {
        private readonly StarcraftDbContext _context;

        public RaceController(StarcraftDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Race>>> GetRace()
        {
            return await _context.Races.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Race>> GetRace(string id)
        {
            var race = await _context.Races.FindAsync(id);

            if (race == null)
            {
                return NotFound();
            }

            return race;
        }
    }
}
