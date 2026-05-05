using System.Text.Json;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Notification
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public NotificationType Type { get; set; }
    public JsonDocument? Payload { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
