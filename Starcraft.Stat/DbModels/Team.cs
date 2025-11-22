using System.ComponentModel.DataAnnotations;
using Starcraft.Stat.Models;

namespace Starcraft.Stat.DbModels;

public class Team
{
    public int Id { get; init; }
    public int Player1Id { get; init; }
    public Player Player1 { get; init; } = null!;

    [MaxLength(10)]
    public string Race1Id { get; init; } = null!;
    public Race Race1 { get; init; } = null!;

    public int Player2Id { get; init; }
    public Player Player2 { get; init; } = null!;
    
    [MaxLength(10)]
    public string Race2Id { get; init; } = null!;
    public Race Race2 { get; init; } = null!;

    public ShortTeam ToShort()
    {
        return string.CompareOrdinal(Player1.Name, Player2.Name) > 0 
            ? new ShortTeam(Player1.Name, Race1Id, Player2.Name, Race2Id) 
            : new ShortTeam(Player2.Name, Race2Id, Player1.Name, Race1Id);
    }
}