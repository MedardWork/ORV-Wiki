using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

public interface IContentTypeRegistry
{
    IReadOnlyCollection<IContentTypeDescriptor> All { get; }
    IContentTypeDescriptor Get(EntityType type);
    bool TryGet(EntityType type, out IContentTypeDescriptor? descriptor);
}
