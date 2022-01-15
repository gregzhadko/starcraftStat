namespace Starcraft.Stat.Models.Responses;

public record PlayerStatisticsResponse(string Name, int Wins) : IPretty
{
    public static string Header => $"{"Wins",-5}Player";
    public string ToPretty() => $"{$"{Wins}",-5}{Name}";
}