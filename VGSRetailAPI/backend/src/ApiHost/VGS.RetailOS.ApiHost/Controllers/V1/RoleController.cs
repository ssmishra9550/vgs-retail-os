using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.Role.Requests;
using VGS.RetailOS.Contracts.V1.Role.Responses;
using VGS.RetailOS.Modules.Role.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1;

[ApiController]
[Route("api/v1/roles")]
[Authorize] // Enforce authentication globally for this controller
public class RoleController : ControllerBase
{
    private readonly IRoleBL _roleBl;

    public RoleController(IRoleBL roleBl)
    {
        _roleBl = roleBl ?? throw new ArgumentNullException(nameof(roleBl));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRolesAsync(CancellationToken cancellationToken)
    {
        var response = await _roleBl.GetRolesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _roleBl.GetRoleByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateRoleAsync([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var response = await _roleBl.CreateRoleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRoleByIdAsync), new { id = response.Id }, response);
    }

    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRoleToUserAsync([FromRoute] Guid id, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        await _roleBl.AssignRoleToUserAsync(id, request, cancellationToken);
        return NoContent();
    }
}
