using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Scenarios.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Scenarios;

public class ScenarioService(
    IPagedEntityRepository<Scenario> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Scenario, ScenarioDto, ScenarioListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Scenario";

    protected override ScenarioDto ToDto(Scenario s, int currentChapter) => new(
        s.Id,
        s.PageId,
        s.Page.Slug,
        Spoilers.RenderInline(s.Page.Title, currentChapter),
        s.Page.DiscoveryChapter,
        Spoilers.RenderInline(s.Page.ShortDescription, currentChapter),
        s.Code,
        Spoilers.RenderInline(s.Title, currentChapter),
        s.Type,
        s.Difficulty,
        Spoilers.RenderInline(s.Conditions, currentChapter),
        Spoilers.RenderInline(s.Rewards, currentChapter),
        Spoilers.RenderInline(s.Penalty, currentChapter),
        s.ChapterStart,
        s.ChapterEnd,
        s.ScenarioLocations
            .OrderBy(sl => sl.Location.Name)
            .Select(sl => new ScenarioLocationDto(
                sl.LocationId, sl.Location.Page.Slug, sl.Location.Name))
            .ToList());

    protected override ScenarioListItemDto ToListItem(Scenario s) => new(
        s.Id,
        s.Page.Slug,
        s.Code,
        s.Title,
        s.Type,
        s.Difficulty,
        s.ChapterStart,
        s.Page.DiscoveryChapter);
}
