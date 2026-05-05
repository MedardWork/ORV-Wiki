namespace ORVWiki.Application.Auth.Dtos;

public record LoginRequest(string EmailOrUsername, string Password);
