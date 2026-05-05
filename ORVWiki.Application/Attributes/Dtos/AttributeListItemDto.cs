using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Attributes.Dtos;

public record AttributeListItemDto(
    long Id,
    string Slug,
    string Name,
    AttributeRarity Rarity,
    int DiscoveryChapter);
