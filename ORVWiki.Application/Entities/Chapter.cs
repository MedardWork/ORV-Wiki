namespace ORVWiki.Application.Entities;

public class Chapter
{
    public int ChapterNumber { get; set; }
    public string? Title { get; set; }
    public long ArcId { get; set; }
    public Arc Arc { get; set; } = null!;
    public string? Summary { get; set; }
    public DateOnly? ReleaseDate { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
