using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

/// <summary>Concise, strongly-typed factory for <see cref="ContentRelation"/>.</summary>
public static class ContentRelations
{
    public static ContentRelation Of<TOwner, TPivot>(
        string name, string label, EntityType targetType,
        Func<TOwner, ICollection<TPivot>> rows,
        Func<TPivot, long> getTargetId, Action<TPivot, long> setTargetId,
        params ContentField[] joinFields)
        where TPivot : class, new()
        => new()
        {
            Name = name, Label = label, TargetType = targetType, JoinFields = joinFields,
            GetRows = o => rows((TOwner)o).Cast<object>(),
            GetTargetEntityId = r => getTargetId((TPivot)r),
            SetTargetEntityId = (r, id) => setTargetId((TPivot)r, id),
            NewRow = () => new TPivot(),
            AddRow = (o, r) => rows((TOwner)o).Add((TPivot)r),
            RemoveRow = (o, r) => rows((TOwner)o).Remove((TPivot)r)
        };
}
