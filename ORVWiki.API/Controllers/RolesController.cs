using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Policy = AuthPolicies.Admin)]
public class RolesController(IAppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll(CancellationToken ct)
    {
        var roles = await db.Roles
            .OrderBy(r => r.Id)
            .Select(r => new RoleDto(r.Id, r.Name))
            .ToListAsync(ct);
        return Ok(roles);
    }

    public record RoleDto(short Id, string Name);
}
