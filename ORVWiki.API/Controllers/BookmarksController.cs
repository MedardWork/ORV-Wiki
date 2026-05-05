using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Bookmarks;
using ORVWiki.Application.Bookmarks.Dtos;
using ORVWiki.Application.Common;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/bookmarks")]
[Authorize(Policy = AuthPolicies.Reader)]
public class BookmarksController(IBookmarkService bookmarks) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<BookmarkDto>>> ListMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.GetId(User);
        return Ok(await bookmarks.ListMineAsync(userId, new PaginationParams(page, pageSize), ct));
    }

    [HttpPost("toggle/{pageId:long}")]
    public async Task<ActionResult<object>> Toggle([FromRoute] long pageId, CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        var added = await bookmarks.ToggleAsync(userId, pageId, ct);
        return Ok(new { bookmarked = added });
    }
}
