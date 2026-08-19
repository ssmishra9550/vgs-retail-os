using Microsoft.AspNetCore.Identity;
using VGS.RetailOS.Contracts.V1.User.Requests;
using VGS.RetailOS.Contracts.V1.User.Responses;
using VGS.RetailOS.Modules.Auth.IBL;
using VGS.RetailOS.Modules.User.BO;
using VGS.RetailOS.Modules.User.IBL;
using VGS.RetailOS.Modules.User.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.User.BL;

public class UserBL : IUserBL
{
    private readonly IUserDAC _userDac;
    private readonly IPasswordVerifier _passwordVerifier;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public UserBL(IUserDAC userDac, IPasswordVerifier passwordVerifier, ITenantContextAccessor tenantContextAccessor)
    {
        _userDac = userDac ?? throw new ArgumentNullException(nameof(userDac));
        _passwordVerifier = passwordVerifier ?? throw new ArgumentNullException(nameof(passwordVerifier));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email is required.");
        }

        // Check if user already exists globally
        var existingUser = await _userDac.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            // If they already exist, just add them to the tenant
            await _userDac.AddUserToTenantAsync(existingUser.Id, tenantId, cancellationToken);
            return MapToResponse(existingUser);
        }

        // Create new user
        var userBo = new UserBO
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var passwordHash = _passwordVerifier.HashPassword(request.Password);
        
        var createdUser = await _userDac.CreateUserAsync(userBo, passwordHash, cancellationToken);
        
        // Add to current tenant
        await _userDac.AddUserToTenantAsync(createdUser.Id, tenantId, cancellationToken);

        return MapToResponse(createdUser);
    }

    public async Task<List<UserResponse>> GetUsersInTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var users = await _userDac.GetAllInTenantAsync(tenantId, cancellationToken);
        return users.Select(MapToResponse).ToList();
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var user = await _userDac.GetByIdAsync(id, tenantId, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found in the current tenant.");
        }

        return MapToResponse(user);
    }

    private string GetTenantId()
    {
        var tenantContext = _tenantContextAccessor.TenantContext;
        if (tenantContext == null || string.IsNullOrWhiteSpace(tenantContext.CurrentTenantId))
        {
            throw new TenantNotFoundException("A valid tenant context is required to perform user operations.");
        }
        return tenantContext.CurrentTenantId;
    }

    private static UserResponse MapToResponse(UserBO bo)
    {
        return new UserResponse
        {
            Id = bo.Id,
            Email = bo.Email,
            FirstName = bo.FirstName,
            LastName = bo.LastName,
            IsActive = bo.IsActive,
            LastLoginAt = bo.LastLoginAt,
            CreatedAt = bo.CreatedAt
        };
    }
}
