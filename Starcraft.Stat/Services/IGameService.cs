using Starcraft.Stat.Models.Requests;

namespace Starcraft.Stat.Services;

public interface IGameService
{
    Task AddGameAsync(AddGameRequest request);
    Task<int> GetGamesCountAsync();
}