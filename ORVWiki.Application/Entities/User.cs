namespace ORVWiki.Application.Entities;

public class User
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int CurrentChapter { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public short RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();
    public ICollection<EditSuggestion> EditSuggestions { get; set; } = new List<EditSuggestion>();
    public ICollection<EditSuggestion> ReviewedEditSuggestions { get; set; } = new List<EditSuggestion>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Media> UploadedMedia { get; set; } = new List<Media>();
}
