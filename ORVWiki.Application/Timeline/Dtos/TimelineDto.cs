namespace ORVWiki.Application.Timeline.Dtos;

public record TimelineDto(
    IReadOnlyList<WorldlineNodeDto> Worldlines,
    IReadOnlyList<EventNodeDto> Events,
    IReadOnlyList<JumpEdgeDto> Jumps);
