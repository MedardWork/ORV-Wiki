using ORVWiki.Application.Auth.Dtos;

namespace ORVWiki.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(long userId, CancellationToken ct = default);
    Task<AuthResponse> UpdateCurrentChapterAsync(long userId, int currentChapter, CancellationToken ct = default);
    Task<UserDto> UpdateRoleAsync(long userId, string roleName, CancellationToken ct = default);
}
