using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ORVWiki.API.Auth;

namespace ORVWiki.API.Realtime;

/// <summary>
/// Real-time inbox. SignalR's default <c>IUserIdProvider</c> reads
/// <c>ClaimTypes.NameIdentifier</c> from the JWT, so we can address connections
/// directly with <c>Clients.User(userId)</c> without manual group bookkeeping.
/// </summary>
[Authorize(Policy = AuthPolicies.Reader)]
public class NotificationsHub : Hub
{
}
