namespace ORVWiki.Application.Tags.Dtos;

public record TagDto(
    short Id,
    string Name,
    string Slug,
    string? Color);
