using FluentValidation;
using ORVWiki.Application.Characters.Dtos;

namespace ORVWiki.Application.Characters.Validators;

public class UpdateCharacterRequestValidator : AbstractValidator<UpdateCharacterRequest>
{
    public UpdateCharacterRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.DiscoveryChapter).GreaterThanOrEqualTo(1);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Alias).MaximumLength(150);
        RuleFor(x => x.Species).MaximumLength(50);

        RuleFor(x => x.BirthChapter).GreaterThanOrEqualTo(1).When(x => x.BirthChapter.HasValue);
        RuleFor(x => x.DeathChapter).GreaterThanOrEqualTo(1).When(x => x.DeathChapter.HasValue);
        RuleFor(x => x)
            .Must(x => !x.DeathChapter.HasValue || !x.BirthChapter.HasValue || x.DeathChapter >= x.BirthChapter)
            .WithMessage("DeathChapter must be >= BirthChapter.");
    }
}
