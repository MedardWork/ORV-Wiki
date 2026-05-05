using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class CommentReaction
{
    public long Id { get; set; }
    public long CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public CommentReactionType ReactionType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
