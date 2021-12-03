using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.DbModels;
using Starcraft.Stat.Models.Responses;

namespace Starcraft.Stat.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly StarcraftDbContext _context;

    public StatisticsController(StarcraftDbContext context)
    {
        _context = context;
    }

    [HttpGet("Player")]
    public async Task<PlayerStatisticsResponse[]> GetPlayersStatistics()
    {
        var games = await _context.Games
            .Include(g => g.Team1).ThenInclude(t => t.Player1)
            .Include(g => g.Team1).ThenInclude(t => t.Player2)
            .Include(g => g.Team2).ThenInclude(t => t.Player1)
            .Include(g => g.Team2).ThenInclude(t => t.Player2)
            .ToArrayAsync();

        var dictionary = new Dictionary<string, int>();
        foreach (var game in games)
        {
            var winnerTeam = game.Winner == Winner.Team1 ? game.Team1 : game.Team2;
            AddOrIncrementDictionaryValue(dictionary, winnerTeam.Player1.Name);
            AddOrIncrementDictionaryValue(dictionary, winnerTeam.Player2.Name);
        }
        
        return dictionary.Select(kv => new PlayerStatisticsResponse(kv.Key, kv.Value)).OrderByDescending(r => r.NumberOfWins).ToArray();
    }

    private static void AddOrIncrementDictionaryValue<T>(IDictionary<T, int> dictionary, T value) where T : notnull
    {
        if (dictionary.ContainsKey(value))
        {
            dictionary[value]++;
        }
        else
        {
            dictionary[value] = 1;
        }
    }
}