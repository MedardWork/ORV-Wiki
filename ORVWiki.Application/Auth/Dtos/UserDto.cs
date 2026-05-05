namespace ORVWiki.Application.Auth.Dtos;

public record UserDto(
    long Id,
    string Email,
    string Username,
    string Role,
    int CurrentChapter,
    string? AvatarUrl,
    string? Bio,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt);
