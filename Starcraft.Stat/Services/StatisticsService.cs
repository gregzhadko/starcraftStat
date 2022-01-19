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

    public async Task<StatisticsResponse> GetPlayerStatisticsAsync(bool showHistory)
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
            .OrderByDescending(g => g.Date)
            .ToArrayAsync();

        var playersDictionary = new Dictionary<string, WinLosses>();
        var teamsDictionary = new Dictionary<(string player1, string player2), WinLosses>();
        var raceDictionary = new Dictionary<(string race1, string race2), WinLosses>();
        var gameResponse = new List<GameResponse>(games.Length);
        foreach (var game in games)
        {
            var (winnerTeam, loserTeam) = game.Winner == Winner.Team1 ? (game.Team1, game.Team2) : (game.Team2, game.Team1);
            AddOrIncrementWinnerLossesDictionary(playersDictionary, winnerTeam.Player1.Name, true);
            AddOrIncrementWinnerLossesDictionary(playersDictionary, winnerTeam.Player2.Name, true);
            AddOrIncrementWinnerLossesDictionary(playersDictionary, loserTeam.Player1.Name, false);
            AddOrIncrementWinnerLossesDictionary(playersDictionary, loserTeam.Player2.Name, false);

            var winners = new[] { winnerTeam.Player1.Name, winnerTeam.Player2.Name }.OrderBy(i => i).ToArray();
            var losers = new[] { loserTeam.Player1.Name, loserTeam.Player2.Name }.OrderBy(i => i).ToArray();
            AddOrIncrementWinnerLossesDictionary(teamsDictionary, (winners[0], winners[1]), true);
            AddOrIncrementWinnerLossesDictionary(teamsDictionary, (losers[0], losers[1]), false);

            FillRacesDictionary(raceDictionary, winnerTeam, loserTeam);

            if (showHistory)
            {
                gameResponse.Add(new GameResponse(game));
            }
        }

        var playersStat = playersDictionary
            .Select(kv => new PlayerStatisticsResponse(kv.Key, kv.Value.Wins, kv.Value.Losses, 100 * (double)kv.Value.Wins / (kv.Value.Losses + kv.Value.Wins)))
            .OrderByDescending(r => r.Wins)
            .ToArray();

        var teamStat = teamsDictionary
            .Select(kv => new TeamStatisticsResponse(kv.Key.player1, kv.Key.player2, kv.Value.Wins, kv.Value.Losses, 100 * (double)kv.Value.Wins / (kv.Value.Losses + kv.Value.Wins)))
            .OrderByDescending(r => r.Wins)
            .ToArray();

        var racesStat = raceDictionary
            .Select(kv =>
            {
                var ((race1, race2), value) = kv;
                return new RacesStatisticsResponse(race1, race2, value.Wins, value.Losses, 100 * (double)value.Wins / (value.Losses + value.Wins));
            })
            .OrderByDescending(r => r.WinRate)
            .ToArray();

        return new StatisticsResponse(playersStat, teamStat, racesStat, gameResponse);
    }

    private static void FillRacesDictionary(IDictionary<(string race1, string race2), WinLosses> raceDictionary, Team winnerTeam, Team loserTeam)
    {
        var winnerRaces = new[] { winnerTeam.Race1.Name, winnerTeam.Race2.Name }.OrderBy(i => i).ToArray();
        var loserRaces = new[] { loserTeam.Race1.Name, loserTeam.Race2.Name }.OrderBy(i => i).ToArray();

        if (winnerRaces.SequenceEqual(loserRaces))
        {
            //We need to add races statistics only in case the pair are different
            return;
        }

        var winValue = (winnerRaces[0], winnerRaces[1]);
        if (raceDictionary.ContainsKey(winValue))
        {
            raceDictionary[winValue].Wins++;
        }
        else
        {
            raceDictionary.Add(winValue, new WinLosses(1, 0));
        }

        var looseValue = (loserRaces[0], loserRaces[1]);
        if (raceDictionary.ContainsKey(looseValue))
        {
            raceDictionary[looseValue].Losses++;
        }
        else
        {
            raceDictionary.Add(looseValue, new WinLosses(0, 1));
        }
    }

    private static void AddOrIncrementWinnerLossesDictionary<T>(IDictionary<T, WinLosses> dictionary, T value, bool winner) where T : notnull
    {
        if (!dictionary.ContainsKey(value))
        {
            dictionary[value] = new WinLosses(0, 0);
        }

        if (winner)
        {
            dictionary[value].Wins++;
        }
        else
        {
            dictionary[value].Losses++;
        }
    }

    private class WinLosses
    {
        public WinLosses(int wins, int losses)
        {
            Wins = wins;
            Losses = losses;
        }

        public int Wins { get; set; }
        public int Losses { get; set; }
    }
}