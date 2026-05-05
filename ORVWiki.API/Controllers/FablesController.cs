using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Fables;
using ORVWiki.Application.Fables.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/fables")]
[Authorize(Policy = AuthPolicies.Reader)]
public class FablesController(FableService fables) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<FableListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await fables.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<FableDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await fables.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<FableDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await fables.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
