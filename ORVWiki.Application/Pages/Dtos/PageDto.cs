using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Pages.Dtos;

public record PageDto(
    long Id,
    string Slug,
    RenderedContent Title,
    EntityType EntityType,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    int ViewCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
