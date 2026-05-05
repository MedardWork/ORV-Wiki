using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Dokkaebis.Dtos;

public record DokkaebiListItemDto(
    long Id,
    string Slug,
    string Name,
    DokkaebiRank Rank,
    int DiscoveryChapter);
