using ORVWiki.Application.Common;

namespace ORVWiki.Application.Content;

/// <summary>
/// The four editable Page-hub fields shared by every content type. Each accessor
/// reaches the satellite entity's <c>Page</c> navigation.
/// </summary>
public static class PageFields
{
    public static ContentField Slug<T>() where T : IPagedEntity
        => ContentFields.Text<T>("slug", "Slug",
            e => e.Page.Slug, (e, v) => e.Page.Slug = v!,
            required: true, nullable: false, maxLength: 120,
            pattern: "^[a-z0-9](-?[a-z0-9])*$", createOnly: true);

    public static ContentField Title<T>() where T : IPagedEntity
        => ContentFields.Text<T>("title", "Title",
            e => e.Page.Title, (e, v) => e.Page.Title = v!,
            required: true, nullable: false, maxLength: 255);

    public static ContentField DiscoveryChapter<T>() where T : IPagedEntity
        => ContentFields.Int<T>("discoveryChapter", "Discovery chapter",
            e => e.Page.DiscoveryChapter, (e, v) => e.Page.DiscoveryChapter = v);

    public static ContentField ShortDescription<T>() where T : IPagedEntity
        => ContentFields.LongText<T>("shortDescription", "Short description",
            e => e.Page.ShortDescription, (e, v) => e.Page.ShortDescription = v,
            maxLength: 2000);

    public static ContentField[] All<T>() where T : IPagedEntity
        => [Slug<T>(), Title<T>(), DiscoveryChapter<T>(), ShortDescription<T>()];
}
