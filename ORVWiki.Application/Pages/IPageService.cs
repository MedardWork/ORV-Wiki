using ORVWiki.Application.Common;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Pages.Dtos;

namespace ORVWiki.Application.Pages;

public interface IPageService
{
    Task<PageDto> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default);

    Task<PaginatedResult<PageDto>> ListVisibleAsync(
        PaginationParams pagination,
        int currentChapter,
        EntityType? entityType,
        string? tagSlug,
        CancellationToken ct = default);
}
