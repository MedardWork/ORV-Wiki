using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Skills.Dtos;

public record SkillDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Name,
    SkillType SkillType,
    int? CostInCoins,
    short? MaxLevel,
    RenderedContent Effect);
