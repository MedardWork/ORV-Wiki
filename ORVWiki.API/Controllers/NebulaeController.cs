using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Nebulae;
using ORVWiki.Application.Nebulae.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/nebulae")]
[Authorize(Policy = AuthPolicies.Reader)]
public class NebulaeController(NebulaService nebulae) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<NebulaListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await nebulae.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<NebulaDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await nebulae.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<NebulaDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await nebulae.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
