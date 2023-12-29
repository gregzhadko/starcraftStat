using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using Starcraft.Stat.DbModels;
using Starcraft.Stat.Exceptions;
using Starcraft.Stat.Models.Requests;
using Throw;

namespace Starcraft.Stat.Services;

public class GameService(StarcraftDbContext context) : IGameService
{
    public async Task AddGameAsync(AddGameRequest request)
    {
        var races = await context.Races.ToArrayAsync();
        var players = await context.Players.ToArrayAsync();

        var team1 = await GetExistingTeamAsync(BuildTeam(request.Team1, races, players));
        var team2 = await GetExistingTeamAsync(BuildTeam(request.Team2, races, players));

        var game = new Game
            { Team1 = team1, Team2 = team2, Winner = request.Winner, Date = DateTime.UtcNow };
        context.Games.Add(game);
        await context.SaveChangesAsync();
    }

    public Task<int> GetGamesCountAsync() => context.Games.CountAsync();

    private static Team BuildTeam(TeamRequest request, IReadOnlyCollection<Race> races, IReadOnlyCollection<Player> players)
    {
        var race1Id = races.FirstOrDefault(r => r.Name.StartsWith(request.Race1, StringComparison.InvariantCultureIgnoreCase))?.Name;
        race1Id.ThrowIfNull(_ => new StarcraftException($"There is no race with '{request.Race1}' name"));
        
        var race2Id = races.FirstOrDefault(r => r.Name.StartsWith(request.Race2, StringComparison.InvariantCultureIgnoreCase))?.Name;
        race2Id.ThrowIfNull(_ => throw new StarcraftException($"There is no race with '{request.Race2}' name"));

        var player1Id = players.FirstOrDefault(r => r.Name.StartsWith(request.Player1, StringComparison.InvariantCultureIgnoreCase))?.Id;
        player1Id.ThrowIfNull(_ => throw new StarcraftException($"There is no player with '{request.Player1}' name"));

        var player2Id = players.FirstOrDefault(r => r.Name.StartsWith(request.Player2, StringComparison.InvariantCultureIgnoreCase))?.Id;
        player2Id.ThrowIfNull(_ => throw new StarcraftException($"There is no player with '{request.Player2}' name"));

        return new()
        {
            Player1Id = player1Id.Value,
            Race1Id = race1Id,
            Player2Id = player2Id.Value,
            Race2Id = race2Id
        };
    }

    private async Task<Team> GetExistingTeamAsync(Team team)
    {
        var existingTeam = await context.Teams.FirstOrDefaultAsync(t =>
            t.Player1Id == team.Player1Id && t.Race1Id == team.Race1Id && t.Player2Id == team.Player2Id &&
            t.Race2Id == team.Race2Id);
        if (existingTeam != null)
        {
            return existingTeam;
        }

        context.Teams.Add(team);
        return team;
    }
}