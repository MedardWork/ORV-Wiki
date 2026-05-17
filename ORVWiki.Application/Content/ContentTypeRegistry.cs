using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

public sealed class ContentTypeRegistry : IContentTypeRegistry
{
    private readonly Dictionary<EntityType, IContentTypeDescriptor> _byType;

    public ContentTypeRegistry(IEnumerable<IContentTypeDescriptor> descriptors)
        => _byType = descriptors.ToDictionary(d => d.EntityType);

    public IReadOnlyCollection<IContentTypeDescriptor> All => _byType.Values;

    public IContentTypeDescriptor Get(EntityType type)
        => _byType.TryGetValue(type, out var d)
            ? d
            : throw new NotFoundException($"No content descriptor registered for '{type}'.");

    public bool TryGet(EntityType type, out IContentTypeDescriptor? descriptor)
        => _byType.TryGetValue(type, out descriptor);
}
