using System.Text.Json;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class EditSuggestion
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;
    public JsonDocument ProposedChanges { get; set; } = null!;
    public string? Reason { get; set; }
    public EditSuggestionStatus Status { get; set; } = EditSuggestionStatus.Pending;
    public long? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
