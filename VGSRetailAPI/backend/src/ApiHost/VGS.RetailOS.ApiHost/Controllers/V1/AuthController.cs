using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.ApiHost.Contracts.V1.Auth;
using VGS.RetailOS.Modules.Auth.BO;
using VGS.RetailOS.Modules.Auth.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthBL _authBl;

    public AuthController(IAuthBL authBl)
    {
        _authBl = authBl;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var command = new LoginCommandBO
        {
            Email = request.Email,
            Password = request.Password,
            TenantHint = request.TenantHint,
            CreatedFromIp = ipAddress,
            UserAgent = userAgent
        };

        var result = await _authBl.LoginAsync(command, cancellationToken);
        
        return Ok(MapToResponse(result));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authBl.RefreshTokenAsync(request.RefreshToken, ipAddress, userAgent, cancellationToken);
        
        return Ok(MapToResponse(result));
    }

    private static AuthResponse MapToResponse(AuthTokenResultBO result)
    {
        return new AuthResponse(
            result.AccessToken,
            result.AccessTokenExpiresAt,
            result.RefreshToken,
            result.RefreshTokenExpiresAt,
            new UserDto(
                result.User.Id,
                result.User.Email,
                result.User.FirstName,
                result.User.LastName
            )
        );
    }
}
