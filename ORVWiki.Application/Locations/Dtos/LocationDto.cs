using System.Text.Json;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Locations.Dtos;

public record LocationDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Name,
    string? Dimension,
    long? WorldlineId,
    long? ParentLocationId,
    JsonDocument? Coordinates,
    RenderedContent Description);
