using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Skills.Dtos;

public record SkillListItemDto(
    long Id,
    string Slug,
    string Name,
    SkillType SkillType,
    int DiscoveryChapter);
