using System.Text.Json;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Notifications.Dtos;

public record NotificationDto(
    long Id,
    NotificationType Type,
    JsonElement? Payload,
    bool IsRead,
    DateTimeOffset CreatedAt);
