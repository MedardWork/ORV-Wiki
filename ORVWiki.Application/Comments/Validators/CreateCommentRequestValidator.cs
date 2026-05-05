using FluentValidation;
using ORVWiki.Application.Comments.Dtos;

namespace ORVWiki.Application.Comments.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.PageId).GreaterThan(0);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0).When(x => x.ParentCommentId.HasValue);
    }
}
