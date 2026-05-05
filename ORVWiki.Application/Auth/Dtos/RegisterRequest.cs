namespace ORVWiki.Application.Auth.Dtos;

public record RegisterRequest(string Email, string Username, string Password, int CurrentChapter = 0);
