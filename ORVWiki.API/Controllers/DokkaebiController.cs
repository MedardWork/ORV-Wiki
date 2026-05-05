using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Dokkaebis;
using ORVWiki.Application.Dokkaebis.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/dokkaebi")]
[Authorize(Policy = AuthPolicies.Reader)]
public class DokkaebiController(DokkaebiService dokkaebi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<DokkaebiListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await dokkaebi.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DokkaebiDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await dokkaebi.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<DokkaebiDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await dokkaebi.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
