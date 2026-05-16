using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Characters;
using ORVWiki.Application.Characters.Dtos;
using ORVWiki.Application.Common;
using ValidationException = ORVWiki.Application.Common.Exceptions.ValidationException;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/characters")]
[Authorize(Policy = AuthPolicies.Reader)]
public class CharactersController(
    ICharacterService characters,
    IValidator<CreateCharacterRequest> createValidator,
    IValidator<UpdateCharacterRequest> updateValidator) : ControllerBase
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

    [HttpPost]
    [Authorize(Policy = AuthPolicies.Editor)]
    public async Task<ActionResult<CharacterDto>> Create(
        [FromBody] CreateCharacterRequest request, CancellationToken ct)
    {
        var v = await createValidator.ValidateAsync(request, ct);
        if (!v.IsValid)
            throw new ValidationException(v.ToDictionary());

        var currentChapter = CurrentUser.GetCurrentChapter(User);
        var dto = await characters.CreateAsync(request, currentChapter, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = AuthPolicies.Editor)]
    public async Task<ActionResult<CharacterDto>> Update(
        [FromRoute] long id, [FromBody] UpdateCharacterRequest request, CancellationToken ct)
    {
        var v = await updateValidator.ValidateAsync(request, ct);
        if (!v.IsValid)
            throw new ValidationException(v.ToDictionary());

        var currentChapter = CurrentUser.GetCurrentChapter(User);
        return Ok(await characters.UpdateAsync(id, request, currentChapter, ct));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
    {
        await characters.DeleteAsync(id, ct);
        return NoContent();
    }
}
