namespace ORVWiki.Application.Timeline.Dtos;

public record WorldlineNodeDto(
    long Id,
    int LineNumber,
    string? Name,
    bool IsMain,
    long? ParentWorldlineId,
    string? Color,
    int DisplayOrder);
