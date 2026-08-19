using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.User.Requests;
using VGS.RetailOS.Contracts.V1.User.Responses;
using VGS.RetailOS.Modules.User.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1;

[ApiController]
[Route("api/v1/users")]
[Authorize] // Enforce authentication globally for this controller
public class UserController : ControllerBase
{
    private readonly IUserBL _userBl;

    public UserController(IUserBL userBl)
    {
        _userBl = userBl ?? throw new ArgumentNullException(nameof(userBl));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var response = await _userBl.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUserByIdAsync), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsersInTenantAsync(CancellationToken cancellationToken)
    {
        var response = await _userBl.GetUsersInTenantAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _userBl.GetUserByIdAsync(id, cancellationToken);
        return Ok(response);
    }
}
