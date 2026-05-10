namespace ORVWiki.Application.Timeline.Dtos;

public record JumpEdgeDto(
    long Id,
    string CharacterLabel,
    string? Description,
    string? LengthEstimate,
    long SourceWorldlineId,
    double SourceOrder,
    long TargetWorldlineId,
    double TargetOrder,
    long? ArcId);
