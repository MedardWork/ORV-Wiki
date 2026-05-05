using ORVWiki.Application.Common;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Notifications.Dtos;

namespace ORVWiki.Application.Notifications;

public interface INotificationService
{
    Task<PaginatedResult<NotificationDto>> ListMineAsync(long userId, PaginationParams p, CancellationToken ct = default);
    Task<int> CountUnreadAsync(long userId, CancellationToken ct = default);
    Task MarkReadAsync(long notificationId, long userId, CancellationToken ct = default);
    Task MarkAllReadAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// Persists a new notification (own SaveChanges) then pushes it via the
    /// real-time channel. Call this AFTER the originating action's own
    /// SaveChanges has succeeded so the notification only lands when the user
    /// actually has something to be notified about.
    /// </summary>
    Task PublishAsync(long userId, NotificationType type, object? payload, CancellationToken ct = default);
}
