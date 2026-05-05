using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;

namespace ORVWiki.Application.Notifications;

public interface INotificationRepository : IRepository<Notification>
{
    Task<PaginatedResult<Notification>> ListMineAsync(long userId, PaginationParams p, CancellationToken ct = default);
    Task<int> CountUnreadAsync(long userId, CancellationToken ct = default);
    Task<Notification?> GetMineAsync(long notificationId, long userId, CancellationToken ct = default);
    Task MarkAllReadAsync(long userId, CancellationToken ct = default);
}
