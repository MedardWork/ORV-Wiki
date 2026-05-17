using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.EditSuggestions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

public class EditSuggestionRepository(AppDbContext db)
    : Repository<EditSuggestion>(db), IEditSuggestionRepository
{
    public Task<EditSuggestion?> GetWithPageAndUsersAsync(long id, CancellationToken ct = default)
        => Db.EditSuggestions
            .Include(s => s.Page)
            .Include(s => s.User)
            .Include(s => s.ReviewedByUser)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<PaginatedResult<EditSuggestion>> ListAsync(
        EditSuggestionStatus? status, PaginationParams p, CancellationToken ct = default)
    {
        var query = Db.EditSuggestions
            .AsNoTracking()
            .Include(s => s.Page)
            .Include(s => s.User)
            .Include(s => s.ReviewedByUser)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip(p.Skip)
            .Take(p.SafePageSize)
            .ToListAsync(ct);

        return new PaginatedResult<EditSuggestion>(items, total, p.SafePage, p.SafePageSize);
    }

    public async Task<PaginatedResult<EditSuggestion>> ListByUserAsync(
        long userId, PaginationParams p, CancellationToken ct = default)
    {
        var query = Db.EditSuggestions
            .AsNoTracking()
            .Include(s => s.Page)
            .Include(s => s.User)
            .Include(s => s.ReviewedByUser)
            .Where(s => s.UserId == userId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip(p.Skip)
            .Take(p.SafePageSize)
            .ToListAsync(ct);

        return new PaginatedResult<EditSuggestion>(items, total, p.SafePage, p.SafePageSize);
    }

    public Task<EntityType?> GetPageEntityTypeAsync(long pageId, CancellationToken ct = default)
        => Db.Pages
            .Where(p => p.Id == pageId)
            .Select(p => (EntityType?)p.EntityType)
            .FirstOrDefaultAsync(ct);
}
