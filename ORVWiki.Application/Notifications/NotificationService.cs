using System.Text.Json;
using Microsoft.Extensions.Logging;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Notifications.Dtos;

namespace ORVWiki.Application.Notifications;

public class NotificationService(
    INotificationRepository notifications,
    INotificationPusher pusher,
    ILogger<NotificationService> logger,
    TimeProvider clock) : INotificationService
{
    public async Task<PaginatedResult<NotificationDto>> ListMineAsync(
        long userId, PaginationParams p, CancellationToken ct = default)
    {
        var result = await notifications.ListMineAsync(userId, p, ct);
        return new PaginatedResult<NotificationDto>(
            result.Items.Select(ToDto).ToList(),
            result.Total,
            result.Page,
            result.PageSize);
    }

    public Task<int> CountUnreadAsync(long userId, CancellationToken ct = default)
        => notifications.CountUnreadAsync(userId, ct);

    public async Task MarkReadAsync(long notificationId, long userId, CancellationToken ct = default)
    {
        var n = await notifications.GetMineAsync(notificationId, userId, ct)
            ?? throw new NotFoundException("Notification not found.");

        if (n.IsRead) return;

        n.IsRead = true;
        await notifications.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(long userId, CancellationToken ct = default)
    {
        await notifications.MarkAllReadAsync(userId, ct);
        await notifications.SaveChangesAsync(ct);
    }

    public async Task PublishAsync(long userId, NotificationType type, object? payload, CancellationToken ct = default)
    {
        JsonDocument? payloadDoc = null;
        if (payload is not null)
        {
            var json = JsonSerializer.Serialize(payload);
            payloadDoc = JsonDocument.Parse(json);
        }

        var entity = new Notification
        {
            UserId = userId,
            Type = type,
            Payload = payloadDoc,
            IsRead = false,
            CreatedAt = clock.GetUtcNow()
        };

        await notifications.AddAsync(entity, ct);
        await notifications.SaveChangesAsync(ct);

        var dto = ToDto(entity);
        try
        {
            await pusher.PushToUserAsync(userId, dto, ct);
        }
        catch (Exception ex)
        {
            // Real-time push is best-effort; the row is already persisted so
            // the user will see it on next fetch.
            logger.LogWarning(ex,
                "Failed to push notification {NotificationId} to user {UserId}",
                entity.Id, userId);
        }
    }

    private static NotificationDto ToDto(Notification n) => new(
        n.Id,
        n.Type,
        n.Payload?.RootElement.Clone(),
        n.IsRead,
        n.CreatedAt);
}
