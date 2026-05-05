using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Characters.Dtos;

public record CharacterListItemDto(
    long Id,
    string Slug,
    string FullName,
    string? Alias,
    CharacterStatus Status,
    int DiscoveryChapter);
