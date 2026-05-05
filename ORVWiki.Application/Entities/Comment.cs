namespace ORVWiki.Application.Entities;

public class Comment
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;
    public long? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public string Body { get; set; } = null!;
    public int ChapterAtPost { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
}
