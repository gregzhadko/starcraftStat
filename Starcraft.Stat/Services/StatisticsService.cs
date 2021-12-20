using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.DbModels;
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
        var raceDictionary = new Dictionary<(string race1, string race2), WinLooses>();
        foreach (var game in games)
        {
            var (winnerTeam, loserTeam) =
                game.Winner == Winner.Team1 ? (game.Team1, game.Team2) : (game.Team2, game.Team1);
            AddOrIncrementDictionaryValue(playersDictionary, winnerTeam.Player1.Name);
            AddOrIncrementDictionaryValue(playersDictionary, winnerTeam.Player2.Name);

            var players = new[] {winnerTeam.Player1.Name, winnerTeam.Player2.Name}.OrderBy(i => i).ToArray();
            AddOrIncrementDictionaryValue(teamsDictionary, (players[0], players[1]));

            FillRacesDictionary(raceDictionary, winnerTeam, loserTeam);
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
            .Select(kv =>
            {
                var ((race1, race2), value) = kv;
                return new RacesStatisticsResponse(race1, race2, value.Wins, value.Looses, 100 * (double)value.Wins / (value.Looses + value.Wins));
            })
            .OrderByDescending(r => r.WinRate)
            .ToArray();

        return new StatisticsResponse(playersStat, teamStat, racesStat);
    }

    private static void FillRacesDictionary(IDictionary<(string race1, string race2), WinLooses> raceDictionary, Team winnerTeam, Team loserTeam)
    {
        var winRaces = new[] {winnerTeam.Race1.Name, winnerTeam.Race2.Name}.OrderBy(i => i).ToArray();
        var looseRaces = new[] {loserTeam.Race1.Name, loserTeam.Race2.Name}.OrderBy(i => i).ToArray();
        if (!winRaces.SequenceEqual(looseRaces)) //We need to add races statistics only in case the pair are different
        {
            var winValue = (winRaces[0], winRaces[1]);
            if (raceDictionary.ContainsKey(winValue))
            {
                raceDictionary[winValue].Wins++;
            }
            else
            {
                raceDictionary.Add(winValue, new WinLooses(1, 0));
            }

            var looseValue = (looseRaces[0], looseRaces[1]);
            if (raceDictionary.ContainsKey(looseValue))
            {
                raceDictionary[looseValue].Looses++;
            }
            else
            {
                raceDictionary.Add(looseValue, new WinLooses(0, 1));
            }
        }
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

    private class WinLooses
    {
        public WinLooses(int wins, int looses)
        {
            Wins = wins;
            Looses = looses;
        }

        public int Wins { get; set; }
        public int Looses { get; set; }
    }
}