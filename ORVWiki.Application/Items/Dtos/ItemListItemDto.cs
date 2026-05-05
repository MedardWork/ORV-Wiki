using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Items.Dtos;

public record ItemListItemDto(
    long Id,
    string Slug,
    string Name,
    ItemGrade ItemGrade,
    bool IsStarRelic,
    int DiscoveryChapter);
