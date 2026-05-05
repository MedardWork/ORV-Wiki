using Microsoft.Extensions.Caching.Memory;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Pages.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Pages;

public class PageService(
    IPageRepository pages,
    ISpoilerService spoilers,
    IMemoryCache cache) : IPageService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<PageDto> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default)
    {
        // Cache by slug only (chapter-independent). Apply the spoiler gate after
        // the cache hit so a single cached row serves every reader.
        var page = await cache.GetOrCreateAsync(PageCacheKeys.BySlug(slug), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await pages.GetBySlugAsync(slug, ct);
        });

        if (page is null || page.DiscoveryChapter > currentChapter)
            throw new NotFoundException($"Page '{slug}' not found.");

        return ToDto(page, currentChapter);
    }

    public async Task<PaginatedResult<PageDto>> ListVisibleAsync(
        PaginationParams pagination,
        int currentChapter,
        EntityType? entityType,
        CancellationToken ct = default)
    {
        var result = await pages.ListVisibleAsync(currentChapter, entityType, pagination, ct);
        return new PaginatedResult<PageDto>(
            result.Items.Select(p => ToDto(p, currentChapter)).ToList(),
            result.Total,
            result.Page,
            result.PageSize);
    }

    private PageDto ToDto(Page p, int currentChapter) => new(
        p.Id,
        p.Slug,
        spoilers.RenderInline(p.Title, currentChapter),
        p.EntityType,
        p.DiscoveryChapter,
        spoilers.RenderInline(p.ShortDescription, currentChapter),
        p.ViewCount,
        p.CreatedAt,
        p.UpdatedAt);
}
