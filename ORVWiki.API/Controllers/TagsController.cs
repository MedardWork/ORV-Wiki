using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Tags;
using ORVWiki.Application.Tags.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/tags")]
[Authorize(Policy = AuthPolicies.Reader)]
public class TagsController(ITagService tags) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> List(CancellationToken ct)
        => Ok(await tags.ListAllAsync(ct));
}
