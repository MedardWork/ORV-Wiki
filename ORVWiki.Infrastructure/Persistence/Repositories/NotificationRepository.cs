using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Notifications;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

public class NotificationRepository(AppDbContext db)
    : Repository<Notification>(db), INotificationRepository
{
    public async Task<PaginatedResult<Notification>> ListMineAsync(
        long userId, PaginationParams p, CancellationToken ct = default)
    {
        var query = Db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(p.Skip)
            .Take(p.SafePageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Notification>(items, total, p.SafePage, p.SafePageSize);
    }

    public Task<int> CountUnreadAsync(long userId, CancellationToken ct = default)
        => Db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public Task<Notification?> GetMineAsync(long notificationId, long userId, CancellationToken ct = default)
        => Db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

    public async Task MarkAllReadAsync(long userId, CancellationToken ct = default)
    {
        await Db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(set => set.SetProperty(n => n.IsRead, true), ct);
    }
}
