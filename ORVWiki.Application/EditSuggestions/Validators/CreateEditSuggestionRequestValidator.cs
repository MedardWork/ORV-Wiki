using System.Text.Json;
using FluentValidation;
using ORVWiki.Application.EditSuggestions.Dtos;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.EditSuggestions.Validators;

public class CreateEditSuggestionRequestValidator : AbstractValidator<CreateEditSuggestionRequest>
{
    public CreateEditSuggestionRequestValidator()
    {
        RuleFor(x => x.Operation)
            .NotEqual(SuggestionOperation.Delete)
            .WithMessage("Deletions cannot be suggested — ask an editor.");

        RuleFor(x => x.ProposedChanges)
            .Must(p => p.ValueKind == JsonValueKind.Object)
            .WithMessage("ProposedChanges must be a JSON object.");

        RuleFor(x => x.PageId)
            .NotNull().GreaterThan(0)
            .When(x => x.Operation != SuggestionOperation.Create)
            .WithMessage("A target page is required.");

        RuleFor(x => x.Reason).MaximumLength(2000);
    }
}
