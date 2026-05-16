using Microsoft.Extensions.Caching.Memory;
using ORVWiki.Application.Characters.Dtos;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Pages;
using ORVWiki.Application.Spoilers;
using ORVWiki.Application.Tags.Dtos;

namespace ORVWiki.Application.Characters;

public class CharacterService(
    ICharacterRepository characters,
    ISpoilerService spoilers,
    IMemoryCache cache,
    TimeProvider clock) : ICharacterService
{
    public async Task<CharacterDetailDto> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default)
    {
        var c = await characters.GetVisibleByIdAsync(id, currentChapter, ct)
            ?? throw new NotFoundException($"Character {id} not found.");
        return ToDetailDto(c, currentChapter);
    }

    public async Task<CharacterDetailDto> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default)
    {
        var c = await characters.GetVisibleBySlugAsync(slug, currentChapter, ct)
            ?? throw new NotFoundException($"Character '{slug}' not found.");
        return ToDetailDto(c, currentChapter);
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

    // Relationship collections arrive already spoiler-filtered from the
    // repository; this only orders and projects them into navigable link DTOs.
    private CharacterDetailDto ToDetailDto(Character c, int currentChapter) => new(
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
        c.PortraitMediaId,
        c.CharacterStigmas
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Stigma.Name)
            .Select(x => new CharacterStigmaDto(
                x.StigmaId, x.Stigma.Page.Slug, x.Stigma.Name, x.IsPrimary, x.AcquiredChapter))
            .ToList(),
        c.CharacterAttributes
            .OrderBy(x => x.Attribute.Name)
            .Select(x => new CharacterAttributeDto(
                x.AttributeId, x.Attribute.Page.Slug, x.Attribute.Name, x.Attribute.Rarity, x.AcquiredChapter))
            .ToList(),
        c.CharacterSkills
            .OrderBy(x => x.Skill.Name)
            .Select(x => new CharacterSkillDto(
                x.SkillId, x.Skill.Page.Slug, x.Skill.Name, x.Skill.SkillType, x.Level, x.AcquiredChapter))
            .ToList(),
        c.CharacterItems
            .OrderBy(x => x.Item.Name)
            .Select(x => new CharacterItemDto(
                x.ItemId, x.Item.Page.Slug, x.Item.Name, x.Item.ItemGrade, x.AcquiredChapter, x.LostChapter))
            .ToList(),
        c.CharacterFables
            .OrderBy(x => x.Fable.Title)
            .Select(x => new CharacterFableDto(
                x.FableId, x.Fable.Page.Slug, x.Fable.Title, x.Fable.Grade, x.AcquiredChapter))
            .ToList(),
        c.CharacterConstellations
            .OrderBy(x => x.RelationshipType)
            .ThenBy(x => x.Constellation.Modifier)
            .Select(x => new CharacterConstellationDto(
                x.ConstellationId, x.Constellation.Page.Slug, x.Constellation.Modifier,
                x.Constellation.TrueName, x.RelationshipType, x.SinceChapter))
            .ToList(),
        c.EventCharacters
            .OrderBy(x => x.Event.ChapterNumber)
            .ThenBy(x => x.Event.Title)
            .Select(x => new CharacterEventDto(
                x.EventId, x.Event.Page.Slug, x.Event.Title, x.Event.ChapterNumber, x.Role))
            .ToList(),
        c.ScenarioParticipants
            .OrderBy(x => x.Scenario.ChapterStart)
            .ThenBy(x => x.Scenario.Title)
            .Select(x => new CharacterScenarioDto(
                x.ScenarioId, x.Scenario.Page.Slug, x.Scenario.Title, x.Scenario.Type, x.Outcome))
            .ToList(),
        c.DeifiedConstellations
            .OrderBy(co => co.Modifier)
            .Select(co => new DeifiedConstellationDto(
                co.Id, co.Page.Slug, co.Modifier, co.TrueName, co.Grade))
            .ToList(),
        c.OriginatedFables
            .OrderBy(f => f.Title)
            .Select(f => new OriginatedFableDto(
                f.Id, f.Page.Slug, f.Title, f.Grade))
            .ToList(),
        c.Page.PageTags
            .OrderBy(pt => pt.Tag.Name)
            .Select(pt => new TagDto(pt.Tag.Id, pt.Tag.Name, pt.Tag.Slug, pt.Tag.Color))
            .ToList());

    private static CharacterListItemDto ToListItem(Character c) => new(
        c.Id,
        c.Page.Slug,
        c.FullName,
        c.Alias,
        c.Status,
        c.Page.DiscoveryChapter);
}
