using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Locations.Dtos;

// A scenario staged at a location — id + slug for navigation, title + type for display.
public record LocationScenarioDto(
    long ScenarioId,
    string Slug,
    string Title,
    ScenarioType Type);
