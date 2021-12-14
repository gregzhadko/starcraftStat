namespace Starcraft.Stat.Models.Responses;

public record StatisticsResponse(PlayerStatisticsResponse[] PlayerStatistics, RacesStatisticsResponse[] RacesStatistics);