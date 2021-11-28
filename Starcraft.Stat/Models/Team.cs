namespace Starcraft.Stat.Models;

public class Team
{
    public int Id { get; set; }
    public int Player1Id { get; set; }
    public Player Player1 { get; set; } = null!;

    public string Race1Id { get; set; } = null!;
    public Race Race1 { get; set; } = null!;

    public int Player2Id { get; set; }
    public Player Player2 { get; set; } = null!;
    public string Race2Id { get; set; } = null!;
    public Race Race2 { get; set; } = null!;
}