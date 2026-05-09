using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.EditSuggestions;
using ORVWiki.Application.EditSuggestions.Dtos;
using ORVWiki.Application.Enums;
using ValidationException = ORVWiki.Application.Common.Exceptions.ValidationException;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/edit-suggestions")]
[Authorize(Policy = AuthPolicies.Reader)]
public class EditSuggestionsController(
    IEditSuggestionService suggestions,
    IValidator<CreateEditSuggestionRequest> createValidator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EditSuggestionDto>> Submit(
        [FromBody] CreateEditSuggestionRequest request, CancellationToken ct)
    {
        var v = await createValidator.ValidateAsync(request, ct);
        if (!v.IsValid)
            throw new ValidationException(v.ToDictionary());

        var userId = CurrentUser.GetId(User);
        var dto = await suggestions.SubmitAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<PaginatedResult<EditSuggestionDto>>> ListMine(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = CurrentUser.GetId(User);
        return Ok(await suggestions.ListMineAsync(userId, new PaginationParams(page, pageSize), ct));
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.Editor)]
    public async Task<ActionResult<PaginatedResult<EditSuggestionDto>>> List(
        [FromQuery] EditSuggestionStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await suggestions.ListAsync(status, new PaginationParams(page, pageSize), ct));

    [HttpGet("{id:long}")]
    [Authorize(Policy = AuthPolicies.Editor)]
    public async Task<ActionResult<EditSuggestionDto>> GetById([FromRoute] long id, CancellationToken ct)
        => Ok(await suggestions.GetAsync(id, ct));

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = AuthPolicies.Editor)]
    public async Task<ActionResult<EditSuggestionDto>> Approve([FromRoute] long id, CancellationToken ct)
    {
        var reviewerId = CurrentUser.GetId(User);
        return Ok(await suggestions.ApproveAsync(id, reviewerId, ct));
    }

    [HttpPost("{id:long}/reject")]
    [Authorize(Policy = AuthPolicies.Editor)]
    public async Task<ActionResult<EditSuggestionDto>> Reject([FromRoute] long id, CancellationToken ct)
    {
        var reviewerId = CurrentUser.GetId(User);
        return Ok(await suggestions.RejectAsync(id, reviewerId, ct));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteOwn([FromRoute] long id, CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        await suggestions.DeleteOwnAsync(id, userId, ct);
        return NoContent();
    }
}
