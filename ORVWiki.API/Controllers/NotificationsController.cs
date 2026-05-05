using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Common;
using ORVWiki.Application.Notifications;
using ORVWiki.Application.Notifications.Dtos;

namespace ORVWiki.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Policy = AuthPolicies.Reader)]
public class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<NotificationDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.GetId(User);
        return Ok(await notifications.ListMineAsync(userId, new PaginationParams(page, pageSize), ct));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> UnreadCount(CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        return Ok(new { count = await notifications.CountUnreadAsync(userId, ct) });
    }

    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> MarkRead([FromRoute] long id, CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        await notifications.MarkReadAsync(id, userId, ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = CurrentUser.GetId(User);
        await notifications.MarkAllReadAsync(userId, ct);
        return NoContent();
    }
}
