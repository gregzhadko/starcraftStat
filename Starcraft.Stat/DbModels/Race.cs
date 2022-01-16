using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Starcraft.Stat.DbModels;

public class Race
{
    public Race(string name)
    {
        Name = name;
    }

    [Key]
    public string Name { get; set; }

    [NotMapped]
    public string ShortName => Name[0].ToString();
}