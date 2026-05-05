using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(AppDbContext db) : IRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext Db = db;
    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    public virtual Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default)
        => Set.FindAsync([id], ct).AsTask();

    public virtual IQueryable<TEntity> Query() => Set.AsQueryable();

    public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public virtual void Update(TEntity entity) => Set.Update(entity);

    public virtual void Remove(TEntity entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Db.SaveChangesAsync(ct);
}
