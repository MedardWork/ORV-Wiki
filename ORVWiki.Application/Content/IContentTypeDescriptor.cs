using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

/// <summary>
/// Declares the editable surface of one content entity type and provides the
/// type-specific persistence hooks the generic engine needs.
/// </summary>
public interface IContentTypeDescriptor
{
    EntityType EntityType { get; }
    string DisplayName { get; }
    IReadOnlyList<ContentField> Fields { get; }
    IReadOnlyList<ContentRelation> Relations { get; }

    /// <summary>Loads the satellite entity (tracked, with Page and relation pivots) by Page id.</summary>
    Task<IPagedEntity?> LoadAsync(IAppDbContext db, long pageId, CancellationToken ct);

    /// <summary>Creates a new satellite entity bound to <paramref name="page"/> and adds it to the context.</summary>
    IPagedEntity CreateNew(IAppDbContext db, Page page);

    /// <summary>Removes the satellite entity; its Page row cascades.</summary>
    void Remove(IAppDbContext db, IPagedEntity entity);

    /// <summary>Resolves a Page id to the satellite entity's own Id.</summary>
    Task<long?> ResolveEntityIdAsync(IAppDbContext db, long pageId, CancellationToken ct);

    /// <summary>Resolves a satellite entity Id back to its Page id.</summary>
    Task<long?> ResolvePageIdAsync(IAppDbContext db, long entityId, CancellationToken ct);

    /// <summary>Optional cross-field validation; returns human-readable error messages.</summary>
    IEnumerable<string> ValidateCrossFields(IPagedEntity entity);
}
