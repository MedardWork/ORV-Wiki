using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Auth;
using ORVWiki.Application.Auth.Dtos;
using ORVWiki.Application.Common.Exceptions;
using ValidationException = ORVWiki.Application.Common.Exceptions.ValidationException;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await registerValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationException(result.ToDictionary());

        var response = await authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Me), new { }, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await loginValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationException(result.ToDictionary());

        return Ok(await authService.LoginAsync(request, ct));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        return Ok(await authService.GetByIdAsync(userId, ct));
    }
}
