using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Worldlines;
using ORVWiki.Application.Worldlines.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/worldlines")]
[Authorize(Policy = AuthPolicies.Reader)]
public class WorldlinesController(WorldlineService worldlines) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<WorldlineListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await worldlines.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<WorldlineDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await worldlines.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<WorldlineDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await worldlines.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
