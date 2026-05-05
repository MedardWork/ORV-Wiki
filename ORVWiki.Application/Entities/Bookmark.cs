namespace ORVWiki.Application.Entities;

public class Bookmark
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
