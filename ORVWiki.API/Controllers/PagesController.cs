using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Pages;
using ORVWiki.Application.Pages.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/pages")]
[Authorize(Policy = AuthPolicies.Reader)]
public class PagesController(IPageService pages) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PageDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] EntityType? entityType = null,
        [FromQuery] string? tag = null,
        [FromQuery] bool ignoreSpoilers = false,
        CancellationToken ct = default)
    {
        var currentChapter = ignoreSpoilers ? int.MaxValue : CurrentUser.GetCurrentChapter(User);
        var result = await pages.ListVisibleAsync(
            new PaginationParams(page, pageSize),
            currentChapter,
            entityType,
            tag,
            ct);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<PageDto>> GetBySlug(
        [FromRoute] string slug,
        [FromQuery] bool ignoreSpoilers = false,
        CancellationToken ct = default)
    {
        var currentChapter = ignoreSpoilers ? int.MaxValue : CurrentUser.GetCurrentChapter(User);
        return Ok(await pages.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
