using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

/// <summary>
/// Concise, strongly-typed factories for <see cref="ContentField"/>. The generic
/// parameter <c>T</c> is the owning object — a content entity or a relation pivot.
/// </summary>
public static class ContentFields
{
    public static ContentField Text<T>(
        string name, string label, Func<T, string?> get, Action<T, string?> set,
        bool required = false, bool nullable = true, int? maxLength = null,
        string? pattern = null, bool createOnly = false)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Text,
            Required = required, Nullable = nullable, MaxLength = maxLength,
            Pattern = pattern, CreateOnly = createOnly,
            Get = o => get((T)o), Set = (o, v) => set((T)o, (string?)v)
        };

    public static ContentField LongText<T>(
        string name, string label, Func<T, string?> get, Action<T, string?> set,
        bool required = false, bool nullable = true, int? maxLength = null)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.LongText,
            Required = required, Nullable = nullable, MaxLength = maxLength,
            Get = o => get((T)o), Set = (o, v) => set((T)o, (string?)v)
        };

    public static ContentField Int<T>(
        string name, string label, Func<T, int> get, Action<T, int> set, bool required = true)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Int,
            Required = required, Nullable = false,
            Get = o => get((T)o), Set = (o, v) => set((T)o, Convert.ToInt32(v))
        };

    public static ContentField IntN<T>(
        string name, string label, Func<T, int?> get, Action<T, int?> set)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Int,
            Required = false, Nullable = true,
            Get = o => get((T)o), Set = (o, v) => set((T)o, v is null ? null : Convert.ToInt32(v))
        };

    public static ContentField Short<T>(
        string name, string label, Func<T, short> get, Action<T, short> set, bool required = true)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Int,
            Required = required, Nullable = false,
            Get = o => get((T)o), Set = (o, v) => set((T)o, Convert.ToInt16(v))
        };

    public static ContentField ShortN<T>(
        string name, string label, Func<T, short?> get, Action<T, short?> set)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Int,
            Required = false, Nullable = true,
            Get = o => get((T)o), Set = (o, v) => set((T)o, v is null ? null : Convert.ToInt16(v))
        };

    public static ContentField Bool<T>(
        string name, string label, Func<T, bool> get, Action<T, bool> set)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Bool,
            Required = false, Nullable = false,
            Get = o => get((T)o), Set = (o, v) => set((T)o, v is bool b && b)
        };

    public static ContentField EnumOf<T, TEnum>(
        string name, string label, Func<T, TEnum> get, Action<T, TEnum> set)
        where TEnum : struct, Enum
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Enum, EnumType = typeof(TEnum),
            Required = true, Nullable = false,
            Get = o => get((T)o), Set = (o, v) => set((T)o, (TEnum)v!)
        };

    public static ContentField EnumOfN<T, TEnum>(
        string name, string label, Func<T, TEnum?> get, Action<T, TEnum?> set)
        where TEnum : struct, Enum
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Enum, EnumType = typeof(TEnum),
            Required = false, Nullable = true,
            Get = o => get((T)o), Set = (o, v) => set((T)o, (TEnum?)v)
        };

    public static ContentField Ref<T>(
        string name, string label, EntityType target, Func<T, long?> get, Action<T, long?> set)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Ref, RefTarget = target,
            Required = false, Nullable = true,
            Get = o => get((T)o), Set = (o, v) => set((T)o, v is null ? null : Convert.ToInt64(v))
        };

    public static ContentField RefReq<T>(
        string name, string label, EntityType target, Func<T, long> get, Action<T, long> set)
        => new()
        {
            Name = name, Label = label, Kind = ContentFieldKind.Ref, RefTarget = target,
            Required = true, Nullable = false,
            Get = o => get((T)o), Set = (o, v) => set((T)o, Convert.ToInt64(v))
        };
}
