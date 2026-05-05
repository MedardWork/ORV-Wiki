using ORVWiki.Application.Entities;

namespace ORVWiki.Application.Auth;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTimeOffset ExpiresAt) Generate(User user);
}
