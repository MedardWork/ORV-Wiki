using ORVWiki.Application.Notifications.Dtos;

namespace ORVWiki.Application.Notifications;

/// <summary>
/// Abstraction for pushing a notification to a connected user in real time.
/// Implemented over SignalR in the API layer; can be no-op'd in tests.
/// </summary>
public interface INotificationPusher
{
    Task PushToUserAsync(long userId, NotificationDto notification, CancellationToken ct = default);
}
