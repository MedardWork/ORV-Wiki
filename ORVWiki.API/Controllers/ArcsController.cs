using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Arcs;
using ORVWiki.Application.Arcs.Dtos;
using ORVWiki.Application.Common;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/arcs")]
[Authorize(Policy = AuthPolicies.Reader)]
public class ArcsController(ArcService arcs) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ArcListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await arcs.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ArcDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await arcs.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<ArcDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await arcs.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
