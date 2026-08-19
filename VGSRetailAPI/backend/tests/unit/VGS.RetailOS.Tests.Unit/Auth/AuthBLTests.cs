using VGS.RetailOS.Modules.Auth.BL;
using VGS.RetailOS.Modules.Auth.BO;
using VGS.RetailOS.Modules.Auth.IBL;
using VGS.RetailOS.Modules.Auth.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Auth;

public class AuthBLTests
{
    private readonly FakeAuthDAC _fakeDac = new();
    private readonly FakePasswordVerifier _fakeVerifier = new();
    private readonly FakeTokenService _fakeTokenService = new();
    private readonly AuthBL _sut;

    public AuthBLTests()
    {
        _sut = new AuthBL(_fakeDac, _fakeVerifier, _fakeTokenService);
    }

    [Fact]
    public async Task LoginAsync_Without_TenantHint_Should_Succeed_Through_Normal_Flow()
    {
        var user = CreateTestUser(isActive: true);
        _fakeDac.Users[user.Email.ToUpperInvariant()] = new UserSecurityInfoBO
        {
            User = user,
            PasswordHash = "HashedPassword",
            AccessFailedCount = 0,
            LockoutEnd = null
        };
        _fakeVerifier.NextResult = PasswordVerificationResult.Success;

        var cmd = new LoginCommandBO { Email = user.Email, Password = "ValidP@ssword123!", TenantHint = null };

        var result = await _sut.LoginAsync(cmd);

        Assert.NotNull(result);
        Assert.Equal("fake_jwt_token", result.AccessToken);
        Assert.Equal("fake_raw_refresh_token", result.RefreshToken);
        Assert.Equal(user.Email, result.User.Email);
        Assert.True(_fakeDac.LoginSuccessRecorded);
    }

    [Fact]
    public async Task LoginAsync_With_TenantHint_Should_Fail_Safely_While_Tenant_Module_Is_Unavailable()
    {
        var user = CreateTestUser(isActive: true);
        _fakeDac.Users[user.Email.ToUpperInvariant()] = new UserSecurityInfoBO
        {
            User = user,
            PasswordHash = "HashedPassword",
            AccessFailedCount = 0,
            LockoutEnd = null
        };

        var cmd = new LoginCommandBO { Email = user.Email, Password = "ValidP@ssword123!", TenantHint = "tenant_123" };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.LoginAsync(cmd));
        Assert.Contains("Tenant-scoped authentication is not supported", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_Null_Or_Empty_Input_Should_Throw_ValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.LoginAsync(null!));
        await Assert.ThrowsAsync<ValidationException>(() => _sut.LoginAsync(new LoginCommandBO { Email = "", Password = "P@ssword123!" }));
        await Assert.ThrowsAsync<ValidationException>(() => _sut.LoginAsync(new LoginCommandBO { Email = "user@test.com", Password = "" }));
    }

    [Fact]
    public async Task LoginAsync_Unknown_User_Should_Throw_UnauthorizedException_Without_Exposing_User_Existence()
    {
        var cmd = new LoginCommandBO { Email = "unknown@test.com", Password = "Password123!" };

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(cmd));
        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_Inactive_User_Should_Throw_UnauthorizedException()
    {
        var user = CreateTestUser(isActive: false);
        _fakeDac.Users[user.Email.ToUpperInvariant()] = new UserSecurityInfoBO
        {
            User = user,
            PasswordHash = "HashedPassword",
            AccessFailedCount = 0,
            LockoutEnd = null
        };

        var cmd = new LoginCommandBO { Email = user.Email, Password = "Password123!" };

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(cmd));
        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_Locked_User_Should_Throw_UnauthorizedException()
    {
        var user = CreateTestUser(isActive: true);
        _fakeDac.Users[user.Email.ToUpperInvariant()] = new UserSecurityInfoBO
        {
            User = user,
            PasswordHash = "HashedPassword",
            AccessFailedCount = 5,
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10)
        };

        var cmd = new LoginCommandBO { Email = user.Email, Password = "Password123!" };

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(cmd));
        Assert.Contains("Account is locked", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_Invalid_Password_Should_Record_Failure()
    {
        var user = CreateTestUser(isActive: true);
        _fakeDac.Users[user.Email.ToUpperInvariant()] = new UserSecurityInfoBO
        {
            User = user,
            PasswordHash = "HashedPassword",
            AccessFailedCount = 2,
            LockoutEnd = null
        };
        _fakeVerifier.NextResult = PasswordVerificationResult.Failed;

        var cmd = new LoginCommandBO { Email = user.Email, Password = "WrongPassword123!" };

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(cmd));
        Assert.Equal("Invalid email or password.", ex.Message);
        Assert.Equal(3, _fakeDac.LastFailedCountRecorded);
    }

    [Fact]
    public async Task LoginAsync_Fifth_Failed_Attempt_Should_Trigger_Lockout()
    {
        var user = CreateTestUser(isActive: true);
        _fakeDac.Users[user.Email.ToUpperInvariant()] = new UserSecurityInfoBO
        {
            User = user,
            PasswordHash = "HashedPassword",
            AccessFailedCount = 4,
            LockoutEnd = null
        };
        _fakeVerifier.NextResult = PasswordVerificationResult.Failed;

        var cmd = new LoginCommandBO { Email = user.Email, Password = "WrongPassword123!" };

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(cmd));
        Assert.Contains("locked due to 5 consecutive failed login attempts", ex.Message);
        Assert.Equal(5, _fakeDac.LastFailedCountRecorded);
        Assert.NotNull(_fakeDac.LastLockoutEndRecorded);
    }

    [Fact]
    public async Task RefreshTokenAsync_Reused_Revoked_Token_Should_Revoke_Entire_Token_Family()
    {
        var familyId = Guid.NewGuid();
        var rawToken = "consumed_token";
        var tokenHash = _fakeTokenService.HashToken(rawToken);

        _fakeDac.RefreshTokens[tokenHash] = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = tokenHash,
            FamilyId = familyId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            IsRevoked = true
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.RefreshTokenAsync(rawToken, "127.0.0.1", "Agent"));
        Assert.Contains("Security Alert", ex.Message);
        Assert.Equal(familyId, _fakeDac.LastRevokedFamilyId);
    }

    [Fact]
    public async Task RefreshTokenAsync_Concurrent_Rotation_Failure_Should_Handle_Safely_And_Revoke_Family()
    {
        var familyId = Guid.NewGuid();
        var rawToken = "active_token";
        var tokenHash = _fakeTokenService.HashToken(rawToken);
        var user = CreateTestUser(isActive: true);

        _fakeDac.RefreshTokens[tokenHash] = new RefreshTokenBO
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            FamilyId = familyId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            IsRevoked = false
        };
        _fakeDac.UserById[user.Id] = user;
        _fakeDac.ShouldFailRotationWithInvalidOperation = true;

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.RefreshTokenAsync(rawToken, "127.0.0.1", "Agent"));
        Assert.Contains("Security Alert: Refresh token has already been rotated", ex.Message);
        Assert.Equal(familyId, _fakeDac.LastRevokedFamilyId);
    }

    [Fact]
    public async Task LogoutAsync_Should_Revoke_Refresh_Token_By_Hash()
    {
        var rawToken = "active_token";
        var expectedHash = _fakeTokenService.HashToken(rawToken);

        await _sut.LogoutAsync(rawToken, "User clicked logout");

        Assert.Equal(expectedHash, _fakeDac.LastRevokedTokenHash);
        Assert.Equal("User clicked logout", _fakeDac.LastRevocationReason);
        Assert.DoesNotContain("active_token", _fakeDac.LastRevokedTokenHash, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCurrentUserAsync_Should_Delegate_To_DAC()
    {
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        _fakeDac.UserById[userId] = user;

        var result = await _sut.GetCurrentUserAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    [Fact]
    public async Task CancellationToken_Should_Propagate_To_DAC()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => _sut.GetCurrentUserAsync(Guid.NewGuid(), cts.Token));
    }

    private static UserBO CreateTestUser(Guid? id = null, bool isActive = true) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Email = "testuser@vgsretail.com",
        FirstName = "Test",
        LastName = "User",
        IsActive = isActive,
        SecurityStamp = Guid.NewGuid().ToString(),
        CreatedAt = DateTimeOffset.UtcNow
    };

    private sealed class FakeAuthDAC : IAuthDAC
    {
        public Dictionary<string, UserSecurityInfoBO> Users { get; } = new();
        public Dictionary<Guid, UserBO> UserById { get; } = new();
        public Dictionary<string, RefreshTokenBO> RefreshTokens { get; } = new();

        public bool LoginSuccessRecorded { get; private set; }
        public int LastFailedCountRecorded { get; private set; }
        public DateTimeOffset? LastLockoutEndRecorded { get; private set; }
        public RefreshTokenBO? SavedRefreshToken { get; private set; }
        public Guid? LastRevokedFamilyId { get; private set; }
        public string? LastRevokedTokenHash { get; private set; }
        public string? LastRevocationReason { get; private set; }
        public bool ShouldFailRotationWithInvalidOperation { get; set; }

        public Task<UserSecurityInfoBO?> FindUserSecurityInfoByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Users.TryGetValue(email.ToUpperInvariant(), out var info);
            return Task.FromResult(info);
        }

        public Task<UserBO?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UserById.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }

        public Task<bool> VerifyTenantMembershipAsync(Guid userId, string tenantIdentifier, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotSupportedException("Tenant membership verification is not supported until the Tenant/Membership domain module is implemented. Silent authorization success is prohibited.");
        }

        public Task RecordLoginSuccessAsync(Guid userId, DateTimeOffset loginTime, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoginSuccessRecorded = true;
            return Task.CompletedTask;
        }

        public Task RecordLoginFailureAsync(Guid userId, int newAccessFailedCount, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastFailedCountRecorded = newAccessFailedCount;
            LastLockoutEndRecorded = lockoutEnd;
            return Task.CompletedTask;
        }

        public Task<RefreshTokenBO?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RefreshTokens.TryGetValue(tokenHash, out var token);
            return Task.FromResult(token);
        }

        public Task SaveRefreshTokenAsync(RefreshTokenBO token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SavedRefreshToken = token;
            RefreshTokens[token.TokenHash] = token;
            return Task.CompletedTask;
        }

        public Task RotateRefreshTokenAsync(string oldTokenHash, RefreshTokenBO newToken, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (ShouldFailRotationWithInvalidOperation)
            {
                throw new InvalidOperationException("Concurrency rotation failure");
            }
            RefreshTokens[newToken.TokenHash] = newToken;
            return Task.CompletedTask;
        }

        public Task RevokeTokenFamilyAsync(Guid familyId, string reason, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRevokedFamilyId = familyId;
            LastRevocationReason = reason;
            return Task.CompletedTask;
        }

        public Task RevokeRefreshTokenAsync(string tokenHash, string reason, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRevokedTokenHash = tokenHash;
            LastRevocationReason = reason;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordVerifier : IPasswordVerifier
    {
        public PasswordVerificationResult NextResult { get; set; } = PasswordVerificationResult.Success;

        public PasswordVerificationResult VerifyPassword(string hashedPassword, string providedPassword)
        {
            // If it's the exact hashed format we use, return Success, otherwise use NextResult for tests that rely on it
            if (hashedPassword == "hashed_" + providedPassword)
                return PasswordVerificationResult.Success;
                
            return NextResult;
        }

        public string HashPassword(string password)
        {
            return "hashed_" + password;
        }
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string HashToken(string token) => $"hash_value_{Math.Abs(token.GetHashCode()):X}";

        public (string RawToken, string TokenHash) GenerateRefreshToken() => ("fake_raw_refresh_token", "hash_value_fake_raw_refresh_token");

        public (string AccessToken, DateTimeOffset ExpiresAt) GenerateAccessToken(UserBO user, string? tenantHint = null) => ("fake_jwt_token", DateTimeOffset.UtcNow.AddMinutes(15));
    }
}
