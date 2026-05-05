using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Scenarios;
using ORVWiki.Application.Scenarios.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/scenarios")]
[Authorize(Policy = AuthPolicies.Reader)]
public class ScenariosController(ScenarioService scenarios) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ScenarioListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await scenarios.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ScenarioDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await scenarios.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<ScenarioDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await scenarios.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
