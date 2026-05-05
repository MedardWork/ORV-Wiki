namespace ORVWiki.Application.Common;

/// <summary>
/// Generic read-only repository for entities that share a 1:1 <see cref="Entities.Page"/>.
/// Applies the spoiler gate (<c>discovery_chapter &lt;= currentChapter</c>) at SQL.
/// </summary>
public interface IPagedEntityRepository<TEntity> where TEntity : class, IPagedEntity
{
    Task<TEntity?> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default);
    Task<TEntity?> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default);

    Task<PaginatedResult<TEntity>> ListVisibleAsync(
        int currentChapter,
        PaginationParams pagination,
        CancellationToken ct = default);
}
