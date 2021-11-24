namespace Starcraft.Stat.Database;

public class Game
{
    public int Id { get; set; }
    public Team Team1 { get; set; }
    public Team Team2 { get; set; }

    public Winner Winner { get; set; }
}

public enum Winner : byte
{
    Team1 = 1,
    Team2 = 2
}