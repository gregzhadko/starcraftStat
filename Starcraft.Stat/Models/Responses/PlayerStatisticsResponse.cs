namespace Starcraft.Stat.Models.Responses;

public record PlayerStatisticsResponse(string Name, int Wins, int Losses, double WinRate) : WinLoseBase(Wins, Losses, WinRate)
{
    public new static string Header => $"{WinLoseBase.Header}Player";
    public override string ToPretty() => $"{base.ToPretty()}{Name}";
}