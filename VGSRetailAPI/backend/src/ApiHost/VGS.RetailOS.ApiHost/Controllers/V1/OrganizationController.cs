using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.Organization.Requests;
using VGS.RetailOS.Contracts.V1.Organization.Responses;
using VGS.RetailOS.Modules.Organization.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1;

[ApiController]
[Route("api/v1/organizations")]
[Authorize]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationBL _organizationBl;

    public OrganizationController(IOrganizationBL organizationBl)
    {
        _organizationBl = organizationBl;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var response = await _organizationBl.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _organizationBl.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var response = await _organizationBl.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }
}
