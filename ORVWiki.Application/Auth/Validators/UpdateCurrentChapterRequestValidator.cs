using FluentValidation;
using ORVWiki.Application.Auth.Dtos;

namespace ORVWiki.Application.Auth.Validators;

public class UpdateCurrentChapterRequestValidator : AbstractValidator<UpdateCurrentChapterRequest>
{
    public UpdateCurrentChapterRequestValidator()
    {
        RuleFor(x => x.CurrentChapter).GreaterThanOrEqualTo(0);
    }
}
