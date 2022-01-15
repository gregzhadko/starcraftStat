namespace Starcraft.Stat.Models.Responses;

public record TeamStatisticsResponse(string Player1, string Player2, int Wins) : IPretty
{
    public static string Header => $"{"Wins",-5}Races";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Player1}",-15}{Player2}";
}