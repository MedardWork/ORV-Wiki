using System.Text.Json;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class EditSuggestion
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public SuggestionOperation Operation { get; set; } = SuggestionOperation.Update;
    public EntityType EntityType { get; set; }

    /// <summary>Target page. Null for a not-yet-approved Create suggestion.</summary>
    public long? PageId { get; set; }
    public Page? Page { get; set; }

    public JsonDocument ProposedChanges { get; set; } = null!;
    public string? Reason { get; set; }
    public EditSuggestionStatus Status { get; set; } = EditSuggestionStatus.Pending;
    public long? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
