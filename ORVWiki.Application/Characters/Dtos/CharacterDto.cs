using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Characters.Dtos;

public record CharacterDto(
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
    long? PortraitMediaId);
