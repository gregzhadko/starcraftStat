using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.DbModels;
using Starcraft.Stat.Models.Requests;
using Starcraft.Stat.Services;

namespace Starcraft.Stat.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly StarcraftDbContext _context;
    private readonly IStatisticsService _statisticsService;

    public GameController(StarcraftDbContext context, IStatisticsService statisticsService)
    {
        _context = context;
        _statisticsService = statisticsService;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> AddGame(AddGameRequest request)
    {
        try
        {
            var races = (await _context.Races.ToArrayAsync()).ToDictionary(r => r.Name, r => r);
            var players = (await _context.Players.ToArrayAsync()).ToDictionary(p => p.Name, p => p);

            var team1 = await GetExistingTeamAsync(BuildTeam(request.Team1, races, players));
            var team2 = await GetExistingTeamAsync(BuildTeam(request.Team2, races, players));

            var game = new Game
                {Team1 = team1, Team2 = team2, Winner = request.Winner, Date = DateOnly.FromDateTime(DateTime.UtcNow)};
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return Ok(await _statisticsService.GetPlayerStatisticsAsync());
        }
        catch
        {
            return BadRequest();
        }
    }

    private static Team BuildTeam(TeamRequest request, IReadOnlyDictionary<string, Race> races,
        IReadOnlyDictionary<string, Player> players)
    {
        return new Team
        {
            Player1Id = players[request.Player1].Id,
            Race1Id = races[request.Race1].Name,
            Player2Id = players[request.Player2].Id,
            Race2Id = races[request.Race2].Name
        };
    }

    private async Task<Team> GetExistingTeamAsync(Team team)
    {
        var existingTeam = await _context.Teams.FirstOrDefaultAsync(t =>
            t.Player1Id == team.Player1Id && t.Race1Id == team.Race1Id && t.Player2Id == team.Player2Id &&
            t.Race2Id == team.Race2Id);
        if (existingTeam != null)
        {
            return existingTeam;
        }

        _context.Teams.Add(team);
        return team;
    }
}