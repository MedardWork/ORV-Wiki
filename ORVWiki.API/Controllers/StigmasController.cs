using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Stigmas;
using ORVWiki.Application.Stigmas.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/stigmas")]
[Authorize(Policy = AuthPolicies.Reader)]
public class StigmasController(StigmaService stigmas) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<StigmaListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await stigmas.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<StigmaDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await stigmas.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<StigmaDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await stigmas.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
