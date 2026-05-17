using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Dtos;

/// <summary>Serializable shape of a <see cref="ContentField"/> for the dynamic form renderer.</summary>
public record ContentFieldDto(
    string Name,
    string Label,
    ContentFieldKind Kind,
    bool Required,
    bool CreateOnly,
    bool Nullable,
    int? MaxLength,
    string? Pattern,
    IReadOnlyList<string> EnumOptions,
    EntityType? RefTarget);

public record ContentRelationDto(
    string Name,
    string Label,
    EntityType TargetType,
    IReadOnlyList<ContentFieldDto> JoinFields);

/// <summary>Serializable content-type schema exposed at <c>GET /api/content-types</c>.</summary>
public record ContentTypeDescriptorDto(
    EntityType EntityType,
    string DisplayName,
    IReadOnlyList<ContentFieldDto> Fields,
    IReadOnlyList<ContentRelationDto> Relations)
{
    public static ContentFieldDto Field(ContentField f) => new(
        f.Name, f.Label, f.Kind, f.Required, f.CreateOnly, f.Nullable,
        f.MaxLength, f.Pattern, f.EnumOptions, f.RefTarget);

    public static ContentTypeDescriptorDto From(IContentTypeDescriptor d) => new(
        d.EntityType,
        d.DisplayName,
        d.Fields.Select(Field).ToList(),
        d.Relations
            .Select(r => new ContentRelationDto(
                r.Name, r.Label, r.TargetType, r.JoinFields.Select(Field).ToList()))
            .ToList());
}
