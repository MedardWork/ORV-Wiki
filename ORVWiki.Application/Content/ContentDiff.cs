using System.Text.Json;

namespace ORVWiki.Application.Content;

/// <summary>Parsed representation of an EditSuggestion's <c>ProposedChanges</c> jsonb.</summary>
public sealed class ContentDiff
{
    public Dictionary<string, JsonElement> Fields { get; } = new();
    public Dictionary<string, RelationDiff> Relations { get; } = new();

    public bool IsEmpty => Fields.Count == 0 && Relations.Count == 0;

    /// <summary>
    /// Accepts the structured shape <c>{ "fields": {...}, "relations": {...} }</c> and,
    /// for backward compatibility with the legacy flat 3-field format, a plain object
    /// (treated entirely as <see cref="Fields"/>).
    /// </summary>
    public static ContentDiff Parse(JsonElement root)
    {
        var diff = new ContentDiff();
        if (root.ValueKind != JsonValueKind.Object) return diff;

        var structured = root.TryGetProperty("fields", out _) || root.TryGetProperty("relations", out _);

        if (structured)
        {
            if (root.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Object)
                foreach (var p in fields.EnumerateObject())
                    diff.Fields[p.Name] = p.Value.Clone();

            if (root.TryGetProperty("relations", out var relations) && relations.ValueKind == JsonValueKind.Object)
                foreach (var p in relations.EnumerateObject())
                    diff.Relations[p.Name] = RelationDiff.Parse(p.Value);
        }
        else
        {
            foreach (var p in root.EnumerateObject())
                diff.Fields[p.Name] = p.Value.Clone();
        }

        return diff;
    }
}

/// <summary>Add / update / remove operations for one relation.</summary>
public sealed class RelationDiff
{
    public List<RelationLinkInput> Add { get; } = new();
    public List<RelationLinkInput> Update { get; } = new();
    public List<long> Remove { get; } = new();

    public static RelationDiff Parse(JsonElement el)
    {
        var d = new RelationDiff();
        if (el.ValueKind != JsonValueKind.Object) return d;

        if (el.TryGetProperty("add", out var add) && add.ValueKind == JsonValueKind.Array)
            foreach (var item in add.EnumerateArray())
                d.Add.Add(RelationLinkInput.Parse(item));

        if (el.TryGetProperty("update", out var upd) && upd.ValueKind == JsonValueKind.Array)
            foreach (var item in upd.EnumerateArray())
                d.Update.Add(RelationLinkInput.Parse(item));

        if (el.TryGetProperty("remove", out var rem) && rem.ValueKind == JsonValueKind.Array)
            foreach (var item in rem.EnumerateArray())
                if (item.ValueKind == JsonValueKind.Number && item.TryGetInt64(out var id))
                    d.Remove.Add(id);

        return d;
    }
}

/// <summary>One relation link in a diff: a target page plus join metadata.</summary>
public sealed class RelationLinkInput
{
    public long TargetPageId { get; set; }
    public Dictionary<string, JsonElement> Metadata { get; } = new();

    public static RelationLinkInput Parse(JsonElement el)
    {
        var link = new RelationLinkInput();
        if (el.ValueKind != JsonValueKind.Object) return link;

        foreach (var p in el.EnumerateObject())
        {
            if (p.Name == "targetPageId")
            {
                if (p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetInt64(out var id))
                    link.TargetPageId = id;
            }
            else
            {
                link.Metadata[p.Name] = p.Value.Clone();
            }
        }

        return link;
    }
}
