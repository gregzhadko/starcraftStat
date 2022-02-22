namespace Starcraft.Stat.Models.Responses;

public record TeamStatisticsResponse(string Player1, string Player2, int Wins, int Losses, double WinRate) : WinLoseBase(Wins, Losses, WinRate)
{
    public new static string Header => $"{WinLoseBase.Header}Teams";
    public override string ToPretty() => $"{base.ToPretty()}{$"{Player1}",-15}{Player2}";
}