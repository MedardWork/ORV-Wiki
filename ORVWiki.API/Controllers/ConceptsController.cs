using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Concepts;
using ORVWiki.Application.Concepts.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/concepts")]
[Authorize(Policy = AuthPolicies.Reader)]
public class ConceptsController(ConceptService concepts) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ConceptListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await concepts.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ConceptDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await concepts.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<ConceptDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await concepts.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
