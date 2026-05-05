using Microsoft.Extensions.Caching.Memory;
using ORVWiki.Application.Characters.Dtos;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Pages;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Characters;

public class CharacterService(
    ICharacterRepository characters,
    ISpoilerService spoilers,
    IMemoryCache cache,
    TimeProvider clock) : ICharacterService
{
    public async Task<CharacterDto> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default)
    {
        var c = await characters.GetVisibleByIdAsync(id, currentChapter, ct)
            ?? throw new NotFoundException($"Character {id} not found.");
        return ToDto(c, currentChapter);
    }

    public async Task<CharacterDto> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default)
    {
        var c = await characters.GetVisibleBySlugAsync(slug, currentChapter, ct)
            ?? throw new NotFoundException($"Character '{slug}' not found.");
        return ToDto(c, currentChapter);
    }

    public async Task<PaginatedResult<CharacterListItemDto>> ListVisibleAsync(
        PaginationParams pagination,
        int currentChapter,
        CancellationToken ct = default)
    {
        var result = await characters.ListVisibleAsync(currentChapter, pagination, ct);
        return new PaginatedResult<CharacterListItemDto>(
            result.Items.Select(ToListItem).ToList(),
            result.Total,
            result.Page,
            result.PageSize);
    }

    public async Task<CharacterDto> CreateAsync(CreateCharacterRequest request, int currentChapter, CancellationToken ct = default)
    {
        if (await characters.SlugExistsAsync(request.Slug, ct))
            throw new ConflictException($"Slug '{request.Slug}' is already in use.");

        var now = clock.GetUtcNow();
        var page = new Page
        {
            Slug = request.Slug,
            Title = request.Title,
            EntityType = EntityType.Character,
            DiscoveryChapter = request.DiscoveryChapter,
            ShortDescription = request.ShortDescription,
            CreatedAt = now,
            UpdatedAt = now
        };

        var character = new Character
        {
            Page = page,
            FullName = request.FullName,
            Alias = request.Alias,
            Species = request.Species ?? "human",
            Status = request.Status,
            Gender = request.Gender,
            BirthChapter = request.BirthChapter,
            DeathChapter = request.DeathChapter,
            Biography = request.Biography,
            PortraitMediaId = request.PortraitMediaId
        };

        await characters.AddAsync(character, ct);
        await characters.SaveChangesAsync(ct);

        return ToDto(character, currentChapter);
    }

    public async Task<CharacterDto> UpdateAsync(long id, UpdateCharacterRequest request, int currentChapter, CancellationToken ct = default)
    {
        var character = await characters.GetWithPageByIdAsync(id, ct)
            ?? throw new NotFoundException($"Character {id} not found.");

        character.Page.Title = request.Title;
        character.Page.DiscoveryChapter = request.DiscoveryChapter;
        character.Page.ShortDescription = request.ShortDescription;
        character.Page.UpdatedAt = clock.GetUtcNow();

        character.FullName = request.FullName;
        character.Alias = request.Alias;
        character.Species = request.Species ?? "human";
        character.Status = request.Status;
        character.Gender = request.Gender;
        character.BirthChapter = request.BirthChapter;
        character.DeathChapter = request.DeathChapter;
        character.Biography = request.Biography;
        character.PortraitMediaId = request.PortraitMediaId;

        await characters.SaveChangesAsync(ct);
        cache.Remove(PageCacheKeys.BySlug(character.Page.Slug));
        return ToDto(character, currentChapter);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var character = await characters.GetWithPageByIdAsync(id, ct)
            ?? throw new NotFoundException($"Character {id} not found.");
        var slug = character.Page.Slug;

        // Removing the Character cascades to its 1:1 Page and pivot rows by FK rules.
        characters.Remove(character);
        await characters.SaveChangesAsync(ct);
        cache.Remove(PageCacheKeys.BySlug(slug));
    }

    private CharacterDto ToDto(Character c, int currentChapter) => new(
        c.Id,
        c.PageId,
        c.Page.Slug,
        spoilers.RenderInline(c.Page.Title, currentChapter),
        c.Page.DiscoveryChapter,
        spoilers.RenderInline(c.Page.ShortDescription, currentChapter),
        c.FullName,
        c.Alias,
        c.Species,
        c.Status,
        c.Gender,
        c.BirthChapter,
        c.DeathChapter,
        spoilers.RenderInline(c.Biography, currentChapter),
        c.PortraitMediaId);

    private static CharacterListItemDto ToListItem(Character c) => new(
        c.Id,
        c.Page.Slug,
        c.FullName,
        c.Alias,
        c.Status,
        c.Page.DiscoveryChapter);
}
