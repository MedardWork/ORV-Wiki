namespace ORVWiki.Application.Content;

/// <summary>Current raw (un-spoiler-rendered) values of a content page, for an edit form.</summary>
public sealed class ContentSnapshot
{
    public long PageId { get; set; }
    public required string EntityType { get; set; }
    public Dictionary<string, object?> Fields { get; } = new();
    public Dictionary<string, List<RelationLinkView>> Relations { get; } = new();
}

/// <summary>One existing relation link, resolved to the target's page for display.</summary>
public sealed class RelationLinkView
{
    public long TargetPageId { get; set; }
    public string? TargetSlug { get; set; }
    public string? TargetTitle { get; set; }
    public Dictionary<string, object?> Metadata { get; } = new();
}
