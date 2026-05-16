using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Characters.Dtos;

// Navigable links from a character to a related entity. Each carries the related
// entity's id + slug (so the client can route to its page) plus the metadata
// stored on the join row. Only relations whose target page is past the reader's
// spoiler gate are included — see CharacterRepository.

public record CharacterStigmaDto(
    long StigmaId,
    string Slug,
    string Name,
    bool IsPrimary,
    int? AcquiredChapter);

public record CharacterAttributeDto(
    long AttributeId,
    string Slug,
    string Name,
    AttributeRarity Rarity,
    int? AcquiredChapter);

public record CharacterSkillDto(
    long SkillId,
    string Slug,
    string Name,
    SkillType SkillType,
    short Level,
    int? AcquiredChapter);

public record CharacterItemDto(
    long ItemId,
    string Slug,
    string Name,
    ItemGrade Grade,
    int? AcquiredChapter,
    int? LostChapter);

public record CharacterFableDto(
    long FableId,
    string Slug,
    string Title,
    FableGrade Grade,
    int? AcquiredChapter);

public record CharacterConstellationDto(
    long ConstellationId,
    string Slug,
    string Modifier,
    string? TrueName,
    CharacterConstellationRel RelationshipType,
    int? SinceChapter);

public record CharacterEventDto(
    long EventId,
    string Slug,
    string Title,
    int ChapterNumber,
    EventCharacterRole Role);

public record CharacterScenarioDto(
    long ScenarioId,
    string Slug,
    string Title,
    ScenarioType Type,
    ScenarioOutcome Outcome);

public record DeifiedConstellationDto(
    long ConstellationId,
    string Slug,
    string Modifier,
    string? TrueName,
    ConstellationGrade Grade);

public record OriginatedFableDto(
    long FableId,
    string Slug,
    string Title,
    FableGrade Grade);
