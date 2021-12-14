using Starcraft.Stat.Models.Responses;

namespace Starcraft.Stat.Services;

public interface IStatisticsService
{
    Task<StatisticsResponse> GetPlayerStatisticsAsync();
}