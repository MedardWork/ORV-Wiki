namespace ORVWiki.Application.Entities;

public class Jump
{
    public long Id { get; set; }

    public string CharacterLabel { get; set; } = null!;
    public string? Description { get; set; }
    public string? LengthEstimate { get; set; }

    public long SourceWorldlineId { get; set; }
    public Worldline SourceWorldline { get; set; } = null!;
    public double SourceOrder { get; set; }

    public long TargetWorldlineId { get; set; }
    public Worldline TargetWorldline { get; set; } = null!;
    public double TargetOrder { get; set; }

    public long? ArcId { get; set; }
    public Arc? Arc { get; set; }
}
