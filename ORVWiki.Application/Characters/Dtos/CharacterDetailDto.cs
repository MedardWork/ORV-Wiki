using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;
using ORVWiki.Application.Tags.Dtos;

namespace ORVWiki.Application.Characters.Dtos;

public record CharacterDetailDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string FullName,
    string? Alias,
    string? Species,
    CharacterStatus Status,
    Gender? Gender,
    int? BirthChapter,
    int? DeathChapter,
    RenderedContent Biography,
    long? PortraitMediaId,
    IReadOnlyList<CharacterStigmaDto> Stigmas,
    IReadOnlyList<CharacterAttributeDto> Attributes,
    IReadOnlyList<CharacterSkillDto> Skills,
    IReadOnlyList<CharacterItemDto> Items,
    IReadOnlyList<CharacterFableDto> Fables,
    IReadOnlyList<CharacterConstellationDto> Constellations,
    IReadOnlyList<CharacterEventDto> Events,
    IReadOnlyList<CharacterScenarioDto> Scenarios,
    IReadOnlyList<DeifiedConstellationDto> DeifiedConstellations,
    IReadOnlyList<OriginatedFableDto> OriginatedFables,
    IReadOnlyList<TagDto> Tags);
