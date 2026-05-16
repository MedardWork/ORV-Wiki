using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;
using ORVWiki.Application.Tags.Dtos;

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
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TagDto> Tags);
