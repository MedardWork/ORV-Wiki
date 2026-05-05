using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Scenarios.Dtos;

public record ScenarioListItemDto(
    long Id,
    string Slug,
    string? Code,
    string Title,
    ScenarioType Type,
    ScenarioDifficulty Difficulty,
    int ChapterStart,
    int DiscoveryChapter);
