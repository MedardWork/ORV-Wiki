using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Timeline;
using ORVWiki.Application.Timeline.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/timeline")]
[Authorize(Policy = AuthPolicies.Reader)]
public class TimelineController(ITimelineService timeline) : ControllerBase
{
    /// <summary>
    /// Returns the full graph payload for the 3D timeline renderer.
    /// </summary>
    /// <param name="upToChapter">Optional chapter cutoff; events past this chapter are excluded.</param>
    /// <param name="characterId">Optional filter — only events involving this character (and the connections between them).</param>
    [HttpGet]
    public async Task<ActionResult<TimelineDto>> Get(
        [FromQuery] int? upToChapter = null,
        [FromQuery] long? characterId = null,
        CancellationToken ct = default)
        => Ok(await timeline.GetGraphAsync(upToChapter, characterId, ct));
}
