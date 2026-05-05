namespace ORVWiki.Application.Entities;

public class Media
{
    public long Id { get; set; }
    public string Url { get; set; } = null!;
    public string? AltText { get; set; }
    public string MimeType { get; set; } = null!;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Character> CharacterPortraits { get; set; } = new List<Character>();
}
