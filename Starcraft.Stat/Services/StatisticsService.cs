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

    public async Task<StatisticsResponse> GetPlayerStatisticsAsync(bool showHistory, bool showTeamPlayerRaceStatistics)
    {
        var games = await LoadFullGamesAsync();

        var playersDictionary = new Dictionary<string, WinLosses>();
        var teamsDictionary = new Dictionary<(string player1, string player2), WinLosses>();
        var raceDictionary = new Dictionary<(string race1, string race2), WinLosses>();
        var playerRaceDictionary = new Dictionary<(string player, string race), WinLosses>();
        var teamPlayerRaceDictionary = new Dictionary<(string player1, string race1, string player2, string race2), WinLosses>();
        var gameResponse = new List<GameResponse>(games.Length);
        foreach (var game in games)
        {
            var (winnerTeam, loserTeam) = game.Winner == Winner.Team1 ? (game.Team1.ToShort(), game.Team2.ToShort()) : (game.Team2.ToShort(), game.Team1.ToShort());
            AddOrIncrementWinnerLossesDictionary(playersDictionary, winnerTeam.Player1, true);
            AddOrIncrementWinnerLossesDictionary(playersDictionary, winnerTeam.Player2, true);
            AddOrIncrementWinnerLossesDictionary(playersDictionary, loserTeam.Player1, false);
            AddOrIncrementWinnerLossesDictionary(playersDictionary, loserTeam.Player2, false);

            AddOrIncrementWinnerLossesDictionary(teamsDictionary, (winnerTeam.Player1, winnerTeam.Player2), true);
            AddOrIncrementWinnerLossesDictionary(teamsDictionary, (loserTeam.Player1, loserTeam.Player2), false);

            AddOrIncrementWinnerLossesDictionary(playerRaceDictionary, (winnerTeam.Player1, winnerTeam.Race1), true);
            AddOrIncrementWinnerLossesDictionary(playerRaceDictionary, (winnerTeam.Player2, winnerTeam.Race2), true);
            AddOrIncrementWinnerLossesDictionary(playerRaceDictionary, (loserTeam.Player1, loserTeam.Race1), false);
            AddOrIncrementWinnerLossesDictionary(playerRaceDictionary, (loserTeam.Player2, loserTeam.Race2), false);

            if (showTeamPlayerRaceStatistics)
            {
                AddOrIncrementWinnerLossesDictionary(teamPlayerRaceDictionary, (winnerTeam.Player1, winnerTeam.Race1, winnerTeam.Player2, winnerTeam.Race2), true);
                AddOrIncrementWinnerLossesDictionary(teamPlayerRaceDictionary, (loserTeam.Player1, loserTeam.Race1, loserTeam.Player2, loserTeam.Race2), false);
            }

            FillRacesDictionary(raceDictionary, winnerTeam, loserTeam);

            if (showHistory)
            {
                gameResponse.Add(new GameResponse(game));
            }
        }

        return BuildStatisticsResponse(playersDictionary, teamsDictionary, raceDictionary, playerRaceDictionary, teamPlayerRaceDictionary, gameResponse);
    }

    private Task<Game[]> LoadFullGamesAsync()
    {
        return _context.Games
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
    }

    private static StatisticsResponse BuildStatisticsResponse(Dictionary<string, WinLosses> playersDictionary, Dictionary<(string player1, string player2), WinLosses> teamsDictionary,
        Dictionary<(string race1, string race2), WinLosses> raceDictionary, Dictionary<(string player, string race), WinLosses> playerRaceDictionary,
        Dictionary<(string player1, string race1, string player2, string race2), WinLosses> teamPlayerRaceDictionary, List<GameResponse> gameResponse)
    {
        var playersStat = playersDictionary
            .Select(kv => new PlayerStatisticsResponse(kv.Key, kv.Value.Wins, kv.Value.Losses, 100 * (double)kv.Value.Wins / (kv.Value.Losses + kv.Value.Wins)))
            .ToArray();
        
        var teamStat = teamsDictionary
            .Select(kv => new TeamStatisticsResponse(kv.Key.player1, kv.Key.player2, kv.Value.Wins, kv.Value.Losses, 100 * (double)kv.Value.Wins / (kv.Value.Losses + kv.Value.Wins)))
            .ToArray();
        
        var racesStat = raceDictionary
            .Select(kv =>
            {
                var ((race1, race2), value) = kv;
                return new RacesStatisticsResponse(race1, race2, value.Wins, value.Losses, 100 * (double)value.Wins / (value.Losses + value.Wins));
            })
            .ToArray();
        
        var playerRaceStat = playerRaceDictionary
            .Select(kv => new PlayerRaceResponse(kv.Key.player, kv.Key.race, kv.Value.Wins, kv.Value.Losses, 100 * (double)kv.Value.Wins / (kv.Value.Losses + kv.Value.Wins)))
            .ToArray();
        
        var teamPlayerRace = GetTeamPlayerRaceStat(teamPlayerRaceDictionary);

        return new StatisticsResponse(playersStat, teamStat, racesStat, gameResponse, playerRaceStat, teamPlayerRace);
    }

    private static TeamPlayerRaceResponse[] GetTeamPlayerRaceStat(Dictionary<(string player1, string race1, string player2, string race2), WinLosses> dict)
    {
        return dict
            .Select(kv =>
            {
                var (key, value) = kv;
                var wins = value.Wins;
                var losses = value.Losses;
                return new TeamPlayerRaceResponse(key.player1, key.race1, key.player2, key.race2, wins, losses, 100 * (double)wins / (losses + wins));
            })
            .ToArray();
    }

    private static void FillRacesDictionary(IDictionary<(string race1, string race2), WinLosses> dict, ShortTeam winnerTeam, ShortTeam loserTeam)
    {
        var winnerRaces = new[] { winnerTeam.Race1, winnerTeam.Race2 }.OrderBy(i => i).ToArray();
        var loserRaces = new[] { loserTeam.Race1, loserTeam.Race2 }.OrderBy(i => i).ToArray();

        if (winnerRaces.SequenceEqual(loserRaces))
        {
            //We need to add races statistics only in case the pair are different
            return;
        }

        var winValue = (winnerRaces[0], winnerRaces[1]);
        if (dict.ContainsKey(winValue))
        {
            dict[winValue].Wins++;
        }
        else
        {
            dict.Add(winValue, new WinLosses(1, 0));
        }

        var looseValue = (loserRaces[0], loserRaces[1]);
        if (dict.ContainsKey(looseValue))
        {
            dict[looseValue].Losses++;
        }
        else
        {
            dict.Add(looseValue, new WinLosses(0, 1));
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