using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Constellations;
using ORVWiki.Application.Constellations.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/constellations")]
[Authorize(Policy = AuthPolicies.Reader)]
public class ConstellationsController(ConstellationService constellations) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ConstellationListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await constellations.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ConstellationDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await constellations.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<ConstellationDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await constellations.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
