using FluentValidation;
using JetBrains.Annotations;

namespace Starcraft.Stat.Models.Requests;

public record AddGameRequest(TeamRequest Team1, TeamRequest Team2, Winner Winner);

[UsedImplicitly]
public class AddGameRequestValidator : AbstractValidator<AddGameRequest>
{
    public AddGameRequestValidator()
    {
        RuleFor(x => x.Winner).IsInEnum();
        RuleFor(x => new[] {x.Team1.Player1, x.Team1.Player2, x.Team2.Player1, x.Team2.Player2})
            .Must(a => a.Distinct().Count() == a.Length)
            .WithMessage("The players names should be unique");
    }
}