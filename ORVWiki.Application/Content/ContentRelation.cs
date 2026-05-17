using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

/// <summary>
/// Describes an editable many-to-many relation (a pivot collection on a content
/// entity). Targets are addressed by the target's Page id; join metadata is
/// described by <see cref="JoinFields"/>.
/// </summary>
public sealed class ContentRelation
{
    public required string Name { get; init; }
    public required string Label { get; init; }
    public required EntityType TargetType { get; init; }

    public IReadOnlyList<ContentField> JoinFields { get; init; } = [];

    /// <summary>The pivot rows currently on the owner entity.</summary>
    public required Func<object, IEnumerable<object>> GetRows { get; init; }

    /// <summary>Reads the target entity id (satellite Id) from a pivot row.</summary>
    public required Func<object, long> GetTargetEntityId { get; init; }

    /// <summary>Writes the target entity id (satellite Id) onto a pivot row.</summary>
    public required Action<object, long> SetTargetEntityId { get; init; }

    /// <summary>Creates a fresh, unattached pivot row.</summary>
    public required Func<object> NewRow { get; init; }

    /// <summary>Adds a pivot row to the owner's collection navigation.</summary>
    public required Action<object, object> AddRow { get; init; }

    /// <summary>Removes a pivot row from the owner's collection navigation.</summary>
    public required Action<object, object> RemoveRow { get; init; }
}
