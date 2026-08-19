using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace VGS.RetailOS.Shared.Auth;

public class UserContextAccessor : IUserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CurrentUserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
