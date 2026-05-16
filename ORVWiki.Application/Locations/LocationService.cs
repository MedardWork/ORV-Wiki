using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Locations.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Locations;

public class LocationService(
    IPagedEntityRepository<Location> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Location, LocationDto, LocationListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Location";

    protected override LocationDto ToDto(Location l, int currentChapter) => new(
        l.Id,
        l.PageId,
        l.Page.Slug,
        Spoilers.RenderInline(l.Page.Title, currentChapter),
        l.Page.DiscoveryChapter,
        Spoilers.RenderInline(l.Page.ShortDescription, currentChapter),
        l.Name,
        l.Dimension,
        l.WorldlineId,
        l.ParentLocationId,
        l.Coordinates,
        Spoilers.RenderInline(l.Description, currentChapter),
        l.ScenarioLocations
            .OrderBy(sl => sl.Scenario.ChapterStart)
            .ThenBy(sl => sl.Scenario.Title)
            .Select(sl => new LocationScenarioDto(
                sl.ScenarioId, sl.Scenario.Page.Slug, sl.Scenario.Title, sl.Scenario.Type))
            .ToList());

    protected override LocationListItemDto ToListItem(Location l) => new(
        l.Id,
        l.Page.Slug,
        l.Name,
        l.Dimension,
        l.Page.DiscoveryChapter);
}
