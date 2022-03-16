namespace Starcraft.Stat.Models.Responses;

public record RacesStatisticsResponse(string Race1, string Race2, int Wins, int Losses, double WinRate) : WinLoseBase(Wins, Losses, WinRate)
{
    public new static string Header => $"{WinLoseBase.Header}Team";
    public override string ToPretty() => $"{base.ToPretty()}{$"{Race1}",-15}{Race2}";
}