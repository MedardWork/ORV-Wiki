using ORVWiki.Application.Bookmarks.Dtos;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Entities;

namespace ORVWiki.Application.Bookmarks;

public class BookmarkService(IBookmarkRepository bookmarks, TimeProvider clock) : IBookmarkService
{
    public async Task<bool> ToggleAsync(long userId, long pageId, CancellationToken ct = default)
    {
        var existing = await bookmarks.GetByUserAndPageAsync(userId, pageId, ct);
        if (existing is not null)
        {
            bookmarks.Remove(existing);
            await bookmarks.SaveChangesAsync(ct);
            return false;
        }

        if (!await bookmarks.PageExistsAsync(pageId, ct))
            throw new NotFoundException($"Page {pageId} not found.");

        await bookmarks.AddAsync(new Bookmark
        {
            UserId = userId,
            PageId = pageId,
            CreatedAt = clock.GetUtcNow()
        }, ct);
        await bookmarks.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PaginatedResult<BookmarkDto>> ListMineAsync(
        long userId, PaginationParams p, CancellationToken ct = default)
    {
        var result = await bookmarks.ListMineAsync(userId, p, ct);
        return new PaginatedResult<BookmarkDto>(
            result.Items.Select(ToDto).ToList(),
            result.Total,
            result.Page,
            result.PageSize);
    }

    private static BookmarkDto ToDto(Bookmark b) => new(
        b.Id,
        b.PageId,
        b.Page.Slug,
        b.Page.Title,
        b.Page.EntityType,
        b.CreatedAt);
}
