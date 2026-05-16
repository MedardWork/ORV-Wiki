using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Scenarios.Dtos;

public record ScenarioDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string? Code,
    RenderedContent ScenarioTitle,
    ScenarioType Type,
    ScenarioDifficulty Difficulty,
    RenderedContent Conditions,
    RenderedContent Rewards,
    RenderedContent Penalty,
    int ChapterStart,
    int? ChapterEnd,
    IReadOnlyList<ScenarioLocationDto> Locations);
