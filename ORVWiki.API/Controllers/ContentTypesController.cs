using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORVWiki.API.Auth;
using ORVWiki.Application.Content;
using ORVWiki.Application.Content.Dtos;

namespace ORVWiki.API.Controllers;

/// <summary>Exposes the content-type schema that drives the dynamic editor/suggestion forms.</summary>
[ApiController]
[Route("api/content-types")]
[Authorize(Policy = AuthPolicies.Reader)]
public class ContentTypesController(IContentTypeRegistry registry) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<ContentTypeDescriptorDto>> List()
        => Ok(registry.All
            .OrderBy(d => d.DisplayName)
            .Select(ContentTypeDescriptorDto.From)
            .ToList());

    [HttpGet("{entityType}")]
    public ActionResult<ContentTypeDescriptorDto> Get([FromRoute] string entityType)
        => Ok(ContentTypeDescriptorDto.From(registry.Get(ContentTypeRouting.Parse(entityType))));
}
