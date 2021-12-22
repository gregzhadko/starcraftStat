using System.Text;

namespace Starcraft.Stat.Models.Responses;

public record StatisticsResponse(PlayerStatisticsResponse[] PlayerStatistics, TeamStatisticsResponse[] TeamStatistics,
    RacesStatisticsResponse[] RacesStatistics) : IPretty
{
    public string ToPretty()
    {
        var result = new StringBuilder();
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

        return result.ToString();
    }
}

public record PlayerStatisticsResponse(string Name, int Wins) : IPretty
{
    public static string Header => $"{"Wins",-5}Player";
    public string ToPretty() => $"{$"{Wins}",-5}{Name}";
}

public record TeamStatisticsResponse(string Player1, string Player2, int Wins) : IPretty
{
    public static string Header => $"{"Wins",-5}Races";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Player1}",-15}{Player2}";
}

public record RacesStatisticsResponse(string Race1, string Race2, int Wins, int Looses, double WinRate) : IPretty
{
    public static string Header => $"{"Wins",-5}{"Looses",-7}{"Win Rate",-11}Team";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Looses}",-7}{$"{WinRate:N}",-11}{$"{Race1}",-15}{Race2}";
}

public interface IPretty
{
    public string ToPretty();
}