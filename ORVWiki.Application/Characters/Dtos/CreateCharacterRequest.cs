using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Characters.Dtos;

public record CreateCharacterRequest(
    string Slug,
    string Title,
    int DiscoveryChapter,
    string? ShortDescription,
    string FullName,
    string? Alias,
    string? Species,
    CharacterStatus Status,
    Gender? Gender,
    int? BirthChapter,
    int? DeathChapter,
    string? Biography,
    long? PortraitMediaId);
