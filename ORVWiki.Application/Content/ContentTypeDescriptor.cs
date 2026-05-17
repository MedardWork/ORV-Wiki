using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

/// <summary>
/// Base for content descriptors: implements the persistence hooks generically.
/// Subclasses declare <see cref="Fields"/>, optional <see cref="Relations"/>,
/// the backing <see cref="Set"/>, and any relation includes.
/// </summary>
public abstract class ContentTypeDescriptor<TEntity> : IContentTypeDescriptor
    where TEntity : class, IPagedEntity, new()
{
    public abstract EntityType EntityType { get; }
    public abstract string DisplayName { get; }
    public abstract IReadOnlyList<ContentField> Fields { get; }
    public virtual IReadOnlyList<ContentRelation> Relations => [];

    protected abstract DbSet<TEntity> Set(IAppDbContext db);

    /// <summary>Adds relation-pivot includes for edit loads. Default: none.</summary>
    protected virtual IQueryable<TEntity> ApplyRelationIncludes(IQueryable<TEntity> query) => query;

    public async Task<IPagedEntity?> LoadAsync(IAppDbContext db, long pageId, CancellationToken ct)
    {
        IQueryable<TEntity> query = Set(db).Include(e => e.Page);
        if (Relations.Count > 0) query = ApplyRelationIncludes(query).AsSplitQuery();
        return await query.FirstOrDefaultAsync(e => e.PageId == pageId, ct);
    }

    public IPagedEntity CreateNew(IAppDbContext db, Page page)
    {
        var entity = new TEntity { Page = page };
        Set(db).Add(entity);
        return entity;
    }

    public void Remove(IAppDbContext db, IPagedEntity entity) => Set(db).Remove((TEntity)entity);

    public async Task<long?> ResolveEntityIdAsync(IAppDbContext db, long pageId, CancellationToken ct)
        => await Set(db).AsNoTracking()
            .Where(e => e.PageId == pageId)
            .Select(e => (long?)e.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<long?> ResolvePageIdAsync(IAppDbContext db, long entityId, CancellationToken ct)
        => await Set(db).AsNoTracking()
            .Where(e => e.Id == entityId)
            .Select(e => (long?)e.PageId)
            .FirstOrDefaultAsync(ct);

    public virtual IEnumerable<string> ValidateCrossFields(IPagedEntity entity) => [];
}
