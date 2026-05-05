using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Auth;
using ORVWiki.Application.Comments;
using ORVWiki.Application.Comments.Dtos;
using ORVWiki.Application.Enums;
using ValidationException = ORVWiki.Application.Common.Exceptions.ValidationException;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/comments")]
[Authorize(Policy = AuthPolicies.Reader)]
public class CommentsController(
    ICommentService comments,
    IValidator<CreateCommentRequest> createValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> ListByPage(
        [FromQuery] long pageId, CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await comments.ListVisibleByPageAsync(pageId, userId, currentChapter, ct));
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> Create(
        [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        var v = await createValidator.ValidateAsync(request, ct);
        if (!v.IsValid)
            throw new ValidationException(v.ToDictionary());

        var userId = CurrentUser.GetId(User);
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        var dto = await comments.CreateAsync(request, userId, currentChapter, ct);
        return CreatedAtAction(nameof(ListByPage), new { pageId = dto.PageId }, dto);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> SoftDelete([FromRoute] long id, CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        var isPrivileged = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Editor);
        await comments.SoftDeleteAsync(id, userId, isPrivileged, ct);
        return NoContent();
    }

    [HttpPost("{id:long}/reactions/{type}")]
    public async Task<ActionResult<object>> ToggleReaction(
        [FromRoute] long id, [FromRoute] CommentReactionType type, CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        var added = await comments.ToggleReactionAsync(id, userId, type, ct);
        return Ok(new { added });
    }
}
