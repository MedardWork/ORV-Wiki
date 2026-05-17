using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Content;
using ORVWiki.Application.Content.Dtos;

namespace ORVWiki.API.Controllers;

/// <summary>Direct editor content management: create / edit / delete any content type.</summary>
[ApiController]
[Route("api/content")]
[Authorize(Policy = AuthPolicies.Editor)]
public class ContentController(IEditorContentService content) : ControllerBase
{
    /// <summary>Raw (un-spoiler-rendered) field + relation values for an edit form.</summary>
    [HttpGet("{entityType}/{pageId:long}")]
    [Authorize(Policy = AuthPolicies.Reader)]
    public async Task<ActionResult<ContentSnapshot>> GetForEdit(
        [FromRoute] string entityType, [FromRoute] long pageId, CancellationToken ct)
        => Ok(await content.GetForEditAsync(ContentTypeRouting.Parse(entityType), pageId, ct));

    [HttpPost("{entityType}")]
    public async Task<ActionResult<ContentWriteResult>> Create(
        [FromRoute] string entityType, [FromBody] ContentWriteRequest request, CancellationToken ct)
        => Ok(await content.CreateAsync(
            ContentTypeRouting.Parse(entityType), request, CurrentUser.GetId(User), ct));

    [HttpPut("{entityType}/{pageId:long}")]
    public async Task<ActionResult<ContentWriteResult>> Update(
        [FromRoute] string entityType, [FromRoute] long pageId,
        [FromBody] ContentWriteRequest request, CancellationToken ct)
        => Ok(await content.UpdateAsync(
            ContentTypeRouting.Parse(entityType), pageId, request, CurrentUser.GetId(User), ct));

    [HttpDelete("{entityType}/{pageId:long}")]
    public async Task<IActionResult> Delete(
        [FromRoute] string entityType, [FromRoute] long pageId, CancellationToken ct)
    {
        await content.DeleteAsync(ContentTypeRouting.Parse(entityType), pageId, CurrentUser.GetId(User), ct);
        return NoContent();
    }
}
