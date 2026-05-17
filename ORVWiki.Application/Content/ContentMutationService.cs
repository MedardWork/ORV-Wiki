using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

/// <summary>
/// The single generic engine that validates and applies a <see cref="ContentDiff"/>
/// for any content type. Used by both the edit-suggestion approval flow and the
/// direct editor content API.
/// </summary>
public sealed class ContentMutationService(
    IAppDbContext db,
    IContentTypeRegistry registry,
    TimeProvider clock) : IContentMutationService
{
    public async Task<IReadOnlyList<string>> ValidateAsync(
        SuggestionOperation operation, IContentTypeDescriptor descriptor,
        ContentDiff diff, long? pageId, CancellationToken ct = default)
    {
        var errors = new List<string>();
        ValidateStructure(operation, descriptor, diff, errors);
        await ValidateSlugAsync(operation, diff, errors, ct);
        return errors;
    }

    public async Task<Page> ApplyAsync(
        SuggestionOperation operation, IContentTypeDescriptor descriptor,
        ContentDiff diff, long? pageId, CancellationToken ct = default)
    {
        var errors = new List<string>();
        ValidateStructure(operation, descriptor, diff, errors);
        await ValidateSlugAsync(operation, diff, errors, ct);
        ThrowIfErrors(errors);

        var now = clock.GetUtcNow();

        if (operation == SuggestionOperation.Delete)
        {
            if (pageId is null) throw new ConflictException("A delete needs a target page.");
            var target = await descriptor.LoadAsync(db, pageId.Value, ct)
                ?? throw new NotFoundException($"{descriptor.DisplayName} page {pageId} not found.");
            var removedPage = target.Page;
            descriptor.Remove(db, target);
            return removedPage;
        }

        IPagedEntity entity;
        Page page;

        if (operation == SuggestionOperation.Create)
        {
            page = new Page
            {
                EntityType = descriptor.EntityType,
                Slug = string.Empty,
                Title = string.Empty,
                DiscoveryChapter = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            entity = descriptor.CreateNew(db, page);
        }
        else
        {
            if (pageId is null) throw new ConflictException("An update needs a target page.");
            entity = await descriptor.LoadAsync(db, pageId.Value, ct)
                ?? throw new NotFoundException($"{descriptor.DisplayName} page {pageId} not found.");
            page = entity.Page;
        }

        await ApplyFieldsAsync(operation, descriptor, diff, entity, errors, ct);
        await ApplyRelationsAsync(descriptor, diff, entity, errors, ct);
        ThrowIfErrors(errors);

        foreach (var msg in descriptor.ValidateCrossFields(entity))
            errors.Add(msg);
        ThrowIfErrors(errors);

        page.UpdatedAt = now;
        return page;
    }

    public async Task<ContentSnapshot> SnapshotAsync(
        IContentTypeDescriptor descriptor, long pageId, CancellationToken ct = default)
    {
        var entity = await descriptor.LoadAsync(db, pageId, ct)
            ?? throw new NotFoundException($"{descriptor.DisplayName} page {pageId} not found.");

        var snapshot = new ContentSnapshot
        {
            PageId = pageId,
            EntityType = descriptor.EntityType.ToString()
        };

        foreach (var field in descriptor.Fields)
            snapshot.Fields[field.Name] = await ProjectForReadAsync(field, field.Get(entity), ct);

        foreach (var rel in descriptor.Relations)
        {
            var targetDesc = registry.Get(rel.TargetType);
            var views = new List<RelationLinkView>();
            foreach (var row in rel.GetRows(entity))
            {
                var targetPageId = await targetDesc.ResolvePageIdAsync(db, rel.GetTargetEntityId(row), ct);
                if (targetPageId is null) continue;
                var info = await db.Pages.AsNoTracking()
                    .Where(p => p.Id == targetPageId)
                    .Select(p => new { p.Slug, p.Title })
                    .FirstOrDefaultAsync(ct);
                var view = new RelationLinkView
                {
                    TargetPageId = targetPageId.Value,
                    TargetSlug = info?.Slug,
                    TargetTitle = info?.Title
                };
                foreach (var jf in rel.JoinFields)
                {
                    var raw = jf.Get(row);
                    view.Metadata[jf.Name] = jf.Kind == ContentFieldKind.Enum ? raw?.ToString() : raw;
                }
                views.Add(view);
            }
            snapshot.Relations[rel.Name] = views;
        }

        return snapshot;
    }

    // ---- validation -------------------------------------------------------

    private static void ValidateStructure(
        SuggestionOperation operation, IContentTypeDescriptor descriptor,
        ContentDiff diff, List<string> errors)
    {
        var fieldsByName = descriptor.Fields.ToDictionary(f => f.Name);

        foreach (var (name, value) in diff.Fields)
        {
            if (!fieldsByName.TryGetValue(name, out var field))
            {
                errors.Add($"Unknown field '{name}'.");
                continue;
            }
            if (operation == SuggestionOperation.Update && field.CreateOnly)
                continue;
            ValidateFieldValue(field, value, errors);
        }

        if (operation == SuggestionOperation.Create)
        {
            foreach (var field in descriptor.Fields)
            {
                if (!field.Required) continue;
                if (!diff.Fields.TryGetValue(field.Name, out var v) ||
                    v.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    errors.Add($"'{field.Label}' is required.");
            }
        }

        var relsByName = descriptor.Relations.Select(r => r.Name).ToHashSet();
        foreach (var (name, _) in diff.Relations)
            if (!relsByName.Contains(name))
                errors.Add($"Unknown relation '{name}'.");
    }

    private static void ValidateFieldValue(ContentField field, JsonElement value, List<string> errors)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            if (!field.Nullable) errors.Add($"'{field.Label}' cannot be empty.");
            return;
        }

        switch (field.Kind)
        {
            case ContentFieldKind.Text:
            case ContentFieldKind.LongText:
                if (value.ValueKind != JsonValueKind.String)
                {
                    errors.Add($"'{field.Label}' must be text.");
                    return;
                }
                var s = value.GetString()!;
                if (field.Required && s.Length == 0)
                    errors.Add($"'{field.Label}' is required.");
                if (field.MaxLength is { } max && s.Length > max)
                    errors.Add($"'{field.Label}' must be at most {max} characters.");
                if (field.Pattern is { } pattern && s.Length > 0 && !Regex.IsMatch(s, pattern))
                    errors.Add($"'{field.Label}' has an invalid format.");
                break;
            case ContentFieldKind.Int:
                if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out _))
                    errors.Add($"'{field.Label}' must be a whole number.");
                break;
            case ContentFieldKind.Bool:
                if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    errors.Add($"'{field.Label}' must be true or false.");
                break;
            case ContentFieldKind.Enum:
                if (value.ValueKind != JsonValueKind.String || field.EnumType is null ||
                    !Enum.TryParse(field.EnumType, value.GetString(), ignoreCase: true, out _))
                    errors.Add($"'{field.Label}' has an invalid value.");
                break;
            case ContentFieldKind.Ref:
                if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out _))
                    errors.Add($"'{field.Label}' must reference a page.");
                break;
        }
    }

    private async Task ValidateSlugAsync(
        SuggestionOperation operation, ContentDiff diff, List<string> errors, CancellationToken ct)
    {
        if (operation != SuggestionOperation.Create) return;
        if (!diff.Fields.TryGetValue("slug", out var slugEl) || slugEl.ValueKind != JsonValueKind.String)
            return;
        var slug = slugEl.GetString()!;
        if (slug.Length > 0 && await db.Pages.AsNoTracking().AnyAsync(p => p.Slug == slug, ct))
            errors.Add($"The slug '{slug}' is already in use.");
    }

    // ---- apply ------------------------------------------------------------

    private async Task ApplyFieldsAsync(
        SuggestionOperation operation, IContentTypeDescriptor descriptor,
        ContentDiff diff, IPagedEntity entity, List<string> errors, CancellationToken ct)
    {
        var fieldsByName = descriptor.Fields.ToDictionary(f => f.Name);
        foreach (var (name, value) in diff.Fields)
        {
            if (!fieldsByName.TryGetValue(name, out var field)) continue;
            if (operation == SuggestionOperation.Update && field.CreateOnly) continue;

            var (ok, coerced) = await CoerceAsync(field, value, errors, ct);
            if (ok) field.Set(entity, coerced);
        }
    }

    private async Task ApplyRelationsAsync(
        IContentTypeDescriptor descriptor, ContentDiff diff,
        IPagedEntity entity, List<string> errors, CancellationToken ct)
    {
        var relsByName = descriptor.Relations.ToDictionary(r => r.Name);

        foreach (var (name, relDiff) in diff.Relations)
        {
            if (!relsByName.TryGetValue(name, out var rel)) continue;
            var targetDesc = registry.Get(rel.TargetType);
            var rows = rel.GetRows(entity).ToList();

            foreach (var targetPageId in relDiff.Remove)
            {
                var targetId = await targetDesc.ResolveEntityIdAsync(db, targetPageId, ct);
                if (targetId is null) continue;
                var row = rows.FirstOrDefault(r => rel.GetTargetEntityId(r) == targetId);
                if (row is not null) { rel.RemoveRow(entity, row); rows.Remove(row); }
            }

            foreach (var link in relDiff.Update)
            {
                var targetId = await targetDesc.ResolveEntityIdAsync(db, link.TargetPageId, ct);
                if (targetId is null)
                {
                    errors.Add($"'{rel.Label}': target page {link.TargetPageId} not found.");
                    continue;
                }
                var row = rows.FirstOrDefault(r => rel.GetTargetEntityId(r) == targetId);
                if (row is null)
                {
                    errors.Add($"'{rel.Label}': no existing link to page {link.TargetPageId}.");
                    continue;
                }
                ApplyJoinFields(rel, row, link, errors);
            }

            foreach (var link in relDiff.Add)
            {
                var targetId = await targetDesc.ResolveEntityIdAsync(db, link.TargetPageId, ct);
                if (targetId is null)
                {
                    errors.Add($"'{rel.Label}': target page {link.TargetPageId} not found.");
                    continue;
                }
                if (rows.Any(r => rel.GetTargetEntityId(r) == targetId)) continue;
                var row = rel.NewRow();
                rel.SetTargetEntityId(row, targetId.Value);
                ApplyJoinFields(rel, row, link, errors);
                rel.AddRow(entity, row);
                rows.Add(row);
            }
        }
    }

    private static void ApplyJoinFields(
        ContentRelation rel, object row, RelationLinkInput link, List<string> errors)
    {
        var joinByName = rel.JoinFields.ToDictionary(f => f.Name);
        foreach (var (metaName, metaValue) in link.Metadata)
        {
            if (!joinByName.TryGetValue(metaName, out var jf)) continue;
            ValidateFieldValue(jf, metaValue, errors);

            if (metaValue.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                jf.Set(row, null);
                continue;
            }

            object? coerced = jf.Kind switch
            {
                ContentFieldKind.Int => metaValue.ValueKind == JsonValueKind.Number ? metaValue.GetInt32() : null,
                ContentFieldKind.Bool => metaValue.ValueKind is JsonValueKind.True or JsonValueKind.False
                    ? metaValue.GetBoolean() : null,
                ContentFieldKind.Enum => jf.EnumType is not null &&
                    Enum.TryParse(jf.EnumType, metaValue.GetString(), ignoreCase: true, out var e) ? e : null,
                _ => metaValue.GetString()
            };
            jf.Set(row, coerced);
        }
    }

    private async Task<(bool ok, object? value)> CoerceAsync(
        ContentField field, JsonElement value, List<string> errors, CancellationToken ct)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return (true, null);

        switch (field.Kind)
        {
            case ContentFieldKind.Text:
            case ContentFieldKind.LongText:
                return (true, value.GetString());
            case ContentFieldKind.Int:
                return (true, value.GetInt32());
            case ContentFieldKind.Bool:
                return (true, value.GetBoolean());
            case ContentFieldKind.Enum:
                if (field.EnumType is not null &&
                    Enum.TryParse(field.EnumType, value.GetString(), ignoreCase: true, out var ev))
                    return (true, ev);
                errors.Add($"'{field.Label}' has an invalid value.");
                return (false, null);
            case ContentFieldKind.Ref:
                if (field.RefTarget is null) return (false, null);
                var targetId = await registry.Get(field.RefTarget.Value)
                    .ResolveEntityIdAsync(db, value.GetInt64(), ct);
                if (targetId is null)
                {
                    errors.Add($"'{field.Label}' references a page that does not exist.");
                    return (false, null);
                }
                return (true, targetId);
            default:
                return (false, null);
        }
    }

    private async Task<object?> ProjectForReadAsync(ContentField field, object? raw, CancellationToken ct)
    {
        if (raw is null) return null;
        return field.Kind switch
        {
            ContentFieldKind.Enum => raw.ToString(),
            ContentFieldKind.Ref => field.RefTarget is null
                ? null
                : await registry.Get(field.RefTarget.Value)
                    .ResolvePageIdAsync(db, Convert.ToInt64(raw), ct),
            _ => raw
        };
    }

    private static void ThrowIfErrors(List<string> errors)
    {
        if (errors.Count == 0) return;
        throw new ValidationException(new Dictionary<string, string[]> { ["content"] = [.. errors] });
    }
}
