using System.ComponentModel.DataAnnotations;

namespace Starcraft.Stat.Database;

public class Race
{
    public Race(string name)
    {
        Name = name;
    }

    [Key]
    public string Name { get; set; }
}