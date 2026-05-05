using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Common;

/// <summary>
/// Base read service for encyclopedic entities that share a 1:1 <see cref="Entities.Page"/>.
/// Subclasses provide the entity-to-DTO mapping (where <see cref="ISpoilerService.RenderInline"/>
/// is applied to narrative fields).
/// </summary>
public abstract class PagedEntityReadService<TEntity, TDto, TListItemDto>(
    IPagedEntityRepository<TEntity> repository,
    ISpoilerService spoilers) where TEntity : class, IPagedEntity
{
    protected ISpoilerService Spoilers => spoilers;

    protected abstract string EntityName { get; }
    protected abstract TDto ToDto(TEntity entity, int currentChapter);
    protected abstract TListItemDto ToListItem(TEntity entity);

    public async Task<TDto> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default)
    {
        var entity = await repository.GetVisibleByIdAsync(id, currentChapter, ct)
            ?? throw new NotFoundException($"{EntityName} {id} not found.");
        return ToDto(entity, currentChapter);
    }

    public async Task<TDto> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default)
    {
        var entity = await repository.GetVisibleBySlugAsync(slug, currentChapter, ct)
            ?? throw new NotFoundException($"{EntityName} '{slug}' not found.");
        return ToDto(entity, currentChapter);
    }

    public async Task<PaginatedResult<TListItemDto>> ListVisibleAsync(
        PaginationParams pagination,
        int currentChapter,
        CancellationToken ct = default)
    {
        var result = await repository.ListVisibleAsync(currentChapter, pagination, ct);
        return new PaginatedResult<TListItemDto>(
            result.Items.Select(ToListItem).ToList(),
            result.Total,
            result.Page,
            result.PageSize);
    }
}
