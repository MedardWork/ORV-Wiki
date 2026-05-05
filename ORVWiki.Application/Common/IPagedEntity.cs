using ORVWiki.Application.Entities;

namespace ORVWiki.Application.Common;

/// <summary>
/// Encyclopedic entity that has a 1:1 relationship with <see cref="Page"/> —
/// shares the spoiler gate (<c>discovery_chapter</c>), slug, title and short
/// description with its page row.
/// </summary>
public interface IPagedEntity
{
    long Id { get; }
    long PageId { get; }
    Page Page { get; }
}
