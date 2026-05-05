using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Auth;
using ORVWiki.Application.Auth.Dtos;
using ValidationException = ORVWiki.Application.Common.Exceptions.ValidationException;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = AuthPolicies.Reader)]
public class UsersController(
    IAuthService authService,
    IValidator<UpdateCurrentChapterRequest> chapterValidator,
    IValidator<UpdateUserRoleRequest> roleValidator) : ControllerBase
{
    [HttpPatch("me/current-chapter")]
    public async Task<ActionResult<AuthResponse>> UpdateMyCurrentChapter(
        [FromBody] UpdateCurrentChapterRequest request, CancellationToken ct)
    {
        var v = await chapterValidator.ValidateAsync(request, ct);
        if (!v.IsValid)
            throw new ValidationException(v.ToDictionary());

        var userId = CurrentUser.GetId(User);
        // Returns a freshly-issued AuthResponse — the JWT carries the new
        // current_chapter claim used by the spoiler gate on /api/pages.
        return Ok(await authService.UpdateCurrentChapterAsync(userId, request.CurrentChapter, ct));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<UserDto>> GetById([FromRoute] long id, CancellationToken ct)
        => Ok(await authService.GetByIdAsync(id, ct));

    [HttpPatch("{id:long}/role")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<UserDto>> UpdateRole(
        [FromRoute] long id, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
    {
        var v = await roleValidator.ValidateAsync(request, ct);
        if (!v.IsValid)
            throw new ValidationException(v.ToDictionary());

        return Ok(await authService.UpdateRoleAsync(id, request.Role, ct));
    }
}
