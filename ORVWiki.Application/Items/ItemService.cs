using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Items.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Items;

public class ItemService(
    IPagedEntityRepository<Item> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Item, ItemDto, ItemListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Item";

    protected override ItemDto ToDto(Item i, int currentChapter) => new(
        i.Id,
        i.PageId,
        i.Page.Slug,
        Spoilers.RenderInline(i.Page.Title, currentChapter),
        i.Page.DiscoveryChapter,
        Spoilers.RenderInline(i.Page.ShortDescription, currentChapter),
        i.Name,
        i.ItemGrade,
        i.IsStarRelic,
        Spoilers.RenderInline(i.Description, currentChapter));

    protected override ItemListItemDto ToListItem(Item i) => new(
        i.Id,
        i.Page.Slug,
        i.Name,
        i.ItemGrade,
        i.IsStarRelic,
        i.Page.DiscoveryChapter);
}
