using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.OuterGods;
using ORVWiki.Application.OuterGods.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/outer-gods")]
[Authorize(Policy = AuthPolicies.Reader)]
public class OuterGodsController(OuterGodService outerGods) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<OuterGodListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await outerGods.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<OuterGodDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await outerGods.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<OuterGodDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await outerGods.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
