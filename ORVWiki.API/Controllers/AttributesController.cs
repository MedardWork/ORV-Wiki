using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Attributes;
using ORVWiki.Application.Attributes.Dtos;
using ORVWiki.Application.Common;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/attributes")]
[Authorize(Policy = AuthPolicies.Reader)]
public class AttributesController(AttributeService attributes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<AttributeListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await attributes.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AttributeDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await attributes.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<AttributeDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await attributes.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
