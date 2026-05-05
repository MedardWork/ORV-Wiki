using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using ORVWiki.Application.Notifications;
using ORVWiki.Application.Notifications.Dtos;

namespace ORVWiki.API.Realtime;

public class SignalRNotificationPusher(IHubContext<NotificationsHub> hub) : INotificationPusher
{
    public const string ClientMethod = "notification";

    public Task PushToUserAsync(long userId, NotificationDto notification, CancellationToken ct = default)
        => hub.Clients
            .User(userId.ToString(CultureInfo.InvariantCulture))
            .SendAsync(ClientMethod, notification, ct);
}
