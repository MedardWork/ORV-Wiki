using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Characters;
using ORVWiki.Application.Characters.Dtos;
using ORVWiki.Application.Common;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/characters")]
[Authorize(Policy = AuthPolicies.Reader)]
public class CharactersController(ICharacterService characters) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<CharacterListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await characters.ListVisibleAsync(new PaginationParams(page, pageSize), currentChapter, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CharacterDetailDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await characters.GetVisibleByIdAsync(id, currentChapter, ct));
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<CharacterDetailDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await characters.GetVisibleBySlugAsync(slug, currentChapter, ct));
    }
}
