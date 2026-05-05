using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Auth.Dtos;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Entities;

namespace ORVWiki.Application.Auth;

public class AuthService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    TimeProvider clock) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var emailTaken = await db.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (emailTaken)
            throw new ConflictException("Email is already registered.");

        var usernameTaken = await db.Users.AnyAsync(u => u.Username == request.Username, ct);
        if (usernameTaken)
            throw new ConflictException("Username is already taken.");

        var readerRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Reader, ct)
            ?? throw new InvalidOperationException($"Default role '{Roles.Reader}' is not seeded.");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = passwordHasher.Hash(request.Password),
            CurrentChapter = Math.Max(0, request.CurrentChapter),
            RoleId = readerRole.Id,
            Role = readerRole,
            IsActive = true,
            CreatedAt = clock.GetUtcNow()
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var (token, expiresAt) = tokenGenerator.Generate(user);
        return new AuthResponse(token, expiresAt, ToDto(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(
                u => u.Email == request.EmailOrUsername || u.Username == request.EmailOrUsername,
                ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new AuthException("Invalid credentials.");

        if (!user.IsActive)
            throw new AuthException("Account is deactivated.");

        user.LastLoginAt = clock.GetUtcNow();
        await db.SaveChangesAsync(ct);

        var (token, expiresAt) = tokenGenerator.Generate(user);
        return new AuthResponse(token, expiresAt, ToDto(user));
    }

    public async Task<UserDto> GetByIdAsync(long userId, CancellationToken ct = default)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");
        return ToDto(user);
    }

    public async Task<AuthResponse> UpdateCurrentChapterAsync(long userId, int currentChapter, CancellationToken ct = default)
    {
        if (currentChapter < 0)
            throw new Common.Exceptions.ValidationException(
                new Dictionary<string, string[]> { ["CurrentChapter"] = ["Must be >= 0."] });

        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        user.CurrentChapter = currentChapter;
        await db.SaveChangesAsync(ct);

        // Re-issue the access token so the embedded current_chapter claim is fresh.
        // The PagesController reads the gate from the JWT, so without this the
        // user's spoiler-visible page set wouldn't change until they logged in again.
        var (token, expiresAt) = tokenGenerator.Generate(user);
        return new AuthResponse(token, expiresAt, ToDto(user));
    }

    public async Task<UserDto> UpdateRoleAsync(long userId, string roleName, CancellationToken ct = default)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct)
            ?? throw new NotFoundException($"Role '{roleName}' not found.");

        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException($"User {userId} not found.");

        user.RoleId = role.Id;
        user.Role = role;
        await db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    private static UserDto ToDto(User u) => new(
        u.Id,
        u.Email,
        u.Username,
        u.Role.Name,
        u.CurrentChapter,
        u.AvatarUrl,
        u.Bio,
        u.IsActive,
        u.CreatedAt,
        u.LastLoginAt);
}
