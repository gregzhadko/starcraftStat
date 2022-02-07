using System.Text;

namespace Starcraft.Stat.Models.Responses;

public record StatisticsResponse(PlayerStatisticsResponse[] PlayerStatistics, TeamStatisticsResponse[] TeamStatistics,
    RacesStatisticsResponse[] RacesStatistics, IReadOnlyCollection<GameResponse> Games, PlayerRaceResponse[] PlayerRaceResponses) : IPretty
{
    private static string Header => "Statistics";

    public string ToPretty()
    {
        var result = new StringBuilder();
        result.AppendLine(Header);
        result.AppendLine();
        result.AppendLine(PlayerStatisticsResponse.Header);
        foreach (var playerStatistic in PlayerStatistics)
        {
            result.AppendLine(playerStatistic.ToPretty());
        }

        result.AppendLine();

        result.AppendLine(TeamStatisticsResponse.Header);
        foreach (var teamStatistic in TeamStatistics)
        {
            result.AppendLine(teamStatistic.ToPretty());
        }

        result.AppendLine();

        result.AppendLine(RacesStatisticsResponse.Header);
        foreach (var racesStatistic in RacesStatistics)
        {
            result.AppendLine(racesStatistic.ToPretty());
        }

        result.AppendLine();

        result.AppendLine(PlayerRaceResponse.Header);
        foreach (var playerRaceResponse in PlayerRaceResponses)
        {
            result.AppendLine(playerRaceResponse.ToPretty());
        }

        if (Games.Count > 0)
        {
            result.AppendLine($"History (total {Games.Count}):");
            foreach (var game in Games)
            {
                result.AppendLine(game.ToPretty());
            }
        }

        return result.ToString();
    }
}