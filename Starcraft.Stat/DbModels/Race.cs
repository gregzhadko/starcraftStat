using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Starcraft.Stat.DbModels;

public class Race(string name)
{
    [Key]
    [MaxLength(10)]
    public string Name { get; set; } = name;

    [NotMapped]
    public string ShortName => Name[0].ToString();
}