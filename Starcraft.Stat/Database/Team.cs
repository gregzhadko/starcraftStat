using System.Collections;
using System.Collections.Generic;

namespace Starcraft.Stat.Database;

public class Team
{
    public Player Player1 { get; set; }
    public Race Race1 { get; set; }
    public Player Player2 { get; set; }
    public Race Race2 { get; set; }
}