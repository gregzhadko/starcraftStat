namespace Starcraft.Stat.Models.Responses;

public record StatisticsResponse(PlayerStatisticsResponse[] PlayerStatistics, TeamStatisticsResponse[] TeamStatistics,
    RacesStatisticsResponse[] RacesStatistics);

public record PlayerStatisticsResponse(string Name, int Wins);

public record RacesStatisticsResponse(string Race1, string Race2, int Wins);

public record TeamStatisticsResponse(string Player1, string Player2, int Wins);