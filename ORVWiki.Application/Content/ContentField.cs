using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

/// <summary>
/// Describes one editable scalar field of a content entity, or one metadata column
/// of a relation pivot. Carries strongly-typed accessors so the engine never reflects.
/// </summary>
public sealed class ContentField
{
    public required string Name { get; init; }
    public required string Label { get; init; }
    public required ContentFieldKind Kind { get; init; }

    /// <summary>Must be present and non-null on Create.</summary>
    public bool Required { get; init; }

    /// <summary>Settable only on Create (e.g. slug); silently ignored in Update diffs.</summary>
    public bool CreateOnly { get; init; }

    /// <summary>Whether a null value is accepted.</summary>
    public bool Nullable { get; init; } = true;

    public int? MaxLength { get; init; }
    public string? Pattern { get; init; }

    /// <summary>CLR enum type when <see cref="Kind"/> is <see cref="ContentFieldKind.Enum"/>.</summary>
    public Type? EnumType { get; init; }

    /// <summary>Target content type when <see cref="Kind"/> is <see cref="ContentFieldKind.Ref"/>.</summary>
    public EntityType? RefTarget { get; init; }

    /// <summary>Reads the raw CLR value from the owning object (entity or pivot row).</summary>
    public required Func<object, object?> Get { get; init; }

    /// <summary>Writes a coerced CLR value onto the owning object.</summary>
    public required Action<object, object?> Set { get; init; }

    public IReadOnlyList<string> EnumOptions => EnumType is null ? [] : Enum.GetNames(EnumType);
}
