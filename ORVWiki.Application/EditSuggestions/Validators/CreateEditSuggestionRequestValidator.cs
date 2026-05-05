using System.Text.Json;
using FluentValidation;
using ORVWiki.Application.EditSuggestions.Dtos;

namespace ORVWiki.Application.EditSuggestions.Validators;

public class CreateEditSuggestionRequestValidator : AbstractValidator<CreateEditSuggestionRequest>
{
    public CreateEditSuggestionRequestValidator()
    {
        RuleFor(x => x.PageId).GreaterThan(0);
        RuleFor(x => x.ProposedChanges)
            .Must(p => p.ValueKind == JsonValueKind.Object)
            .WithMessage("ProposedChanges must be a JSON object.");
        RuleFor(x => x.Reason).MaximumLength(2000);
    }
}
