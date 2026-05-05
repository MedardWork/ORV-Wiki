namespace ORVWiki.Application.Auth.Dtos;

public record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, UserDto User);
