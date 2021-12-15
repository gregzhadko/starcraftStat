using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.Models;
using Starcraft.Stat.Models.Responses;

namespace Starcraft.Stat.Services;

public class StatisticsService : IStatisticsService
{
    private readonly StarcraftDbContext _context;

    public StatisticsService(StarcraftDbContext context)
    {
        _context = context;
    }

    public async Task<StatisticsResponse> GetPlayerStatisticsAsync()
    {
        var games = await _context.Games
            .Include(g => g.Team1.Player1)
            .Include(g => g.Team1.Race1)
            .Include(g => g.Team1.Player2)
            .Include(g => g.Team1.Race2)
            .Include(g => g.Team2.Player1)
            .Include(g => g.Team2.Race1)
            .Include(g => g.Team2.Player2)
            .Include(g => g.Team2.Race2)
            .ToArrayAsync();

        var playersDictionary = new Dictionary<string, int>();
        var teamsDictionary = new Dictionary<(string player1, string player2), int>();
        var raceDictionary = new Dictionary<(string race1, string race2), int>();
        foreach (var game in games)
        {
            var (winnerTeam, loserTeam) = game.Winner == Winner.Team1 ? (game.Team1, game.Team2) : (game.Team2, game.Team1);
            AddOrIncrementDictionaryValue(playersDictionary, winnerTeam.Player1.Name);
            AddOrIncrementDictionaryValue(playersDictionary, winnerTeam.Player2.Name);

            var players = new[] {winnerTeam.Player1.Name, winnerTeam.Player2.Name}.OrderBy(i => i).ToArray();
            AddOrIncrementDictionaryValue(teamsDictionary, (players[0], players[1]));
            
            if (!new[] {winnerTeam.Race1.Name, winnerTeam.Race2.Name}.OrderBy(i => i)
                    .SequenceEqual(new[] {loserTeam.Race1.Name, loserTeam.Race2.Name}))
            {
                //We need to add races statistics only in case the pair are different
                var races = new[] {winnerTeam.Race1.Name, winnerTeam.Race2.Name}.OrderBy(i => i).ToArray();
                AddOrIncrementDictionaryValue(raceDictionary, (races[0], races[1]));
            }
        }

        var playersStat = playersDictionary
            .Select(kv => new PlayerStatisticsResponse(kv.Key, kv.Value))
            .OrderByDescending(r => r.Wins)
            .ToArray();
        
        var teamStat = teamsDictionary
            .Select(kv => new TeamStatisticsResponse(kv.Key.player1, kv.Key.player2, kv.Value))
            .OrderByDescending(r => r.Wins)
            .ToArray();

        var racesStat = raceDictionary
            .Select(kv => new RacesStatisticsResponse(kv.Key.race1, kv.Key.race2, kv.Value))
            .OrderByDescending(r => r.Wins)
            .ToArray();

        return new StatisticsResponse(playersStat, teamStat, racesStat);
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