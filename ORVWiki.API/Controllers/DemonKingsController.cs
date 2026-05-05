using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.DemonKings;
using ORVWiki.Application.DemonKings.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/demon-kings")]
[Authorize(Policy = AuthPolicies.Reader)]
public class DemonKingsController(DemonKingService demonKings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<DemonKingListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await demonKings.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DemonKingDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await demonKings.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<DemonKingDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await demonKings.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
