namespace Starcraft.Stat.Models.Responses;

public record RacesStatisticsResponse(string Race1, string Race2, int Wins, int Looses, double WinRate) : IPretty
{
    public static string Header => $"{"Wins",-5}{"Looses",-7}{"Win Rate",-11}Team";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Looses}",-7}{$"{WinRate:N}",-11}{$"{Race1}",-15}{Race2}";
}