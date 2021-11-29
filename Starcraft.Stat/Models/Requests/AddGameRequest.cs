namespace Starcraft.Stat.Models.Requests;

public class AddGameRequest
{
    public TeamRequest Team1 { get; set; }
    public TeamRequest Team2 { get; set; }
    
    //The first team is the winner
}