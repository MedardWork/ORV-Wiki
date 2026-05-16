namespace ORVWiki.Application.Scenarios.Dtos;

// A location a scenario plays out in — id + slug for navigation, name for display.
public record ScenarioLocationDto(
    long LocationId,
    string Slug,
    string Name);
