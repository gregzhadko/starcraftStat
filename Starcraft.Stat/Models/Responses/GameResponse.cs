using JetBrains.Annotations;
using Starcraft.Stat.DbModels;

namespace Starcraft.Stat.Models.Responses;

[PublicAPI]
public record GameResponse : IPretty
{
    public GameResponse(Game game)
    {
        Team winnerTeam;
        Team loserTeam;
        if (game.Winner == Winner.Team1)
        {
            winnerTeam = game.Team1;
            loserTeam = game.Team2;
        }
        else
        {
            winnerTeam = game.Team2;
            loserTeam = game.Team1;
        }

        WinnerPlayer1 = winnerTeam.Player1.Name;
        WinnerPlayer2 = winnerTeam.Player2.Name;
        WinnerRace1 = winnerTeam.Race1.ShortName;
        WinnerRace2 = winnerTeam.Race2.ShortName;

        LoserPlayer1 = loserTeam.Player1.Name;
        LoserPlayer2 = loserTeam.Player2.Name;
        LoserRace1 = loserTeam.Race1.ShortName;
        LoserRace2 = loserTeam.Race2.ShortName;
    }

    public string WinnerPlayer1 { get; }

    public string WinnerRace1 { get; }

    public string WinnerPlayer2 { get; }

    public string WinnerRace2 { get; }

    public string LoserPlayer1 { get; }

    public string LoserRace1 { get; }

    public string LoserPlayer2 { get; }

    public string LoserRace2 { get; }
    public static string Header => "History:";

    public string ToPretty() => $"{GetPretty(WinnerPlayer1, WinnerRace1)} {GetPretty(WinnerPlayer2, WinnerRace2)} 1 : 0  {GetPretty(LoserPlayer1, LoserRace1)} {GetPretty(LoserPlayer2, LoserRace2)}";

    private static string GetPretty(string player, string race) => $"{$"{player} ({race})",-16}";
}