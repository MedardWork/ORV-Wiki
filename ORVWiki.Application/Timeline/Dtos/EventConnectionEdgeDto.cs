using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Timeline.Dtos;

public record EventConnectionEdgeDto(
    long Id,
    long SourceEventId,
    long TargetEventId,
    EventConnectionType ConnectionType,
    long? CharacterId,
    string? Description);
