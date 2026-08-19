using Moq;
using VGS.RetailOS.Contracts.V1.Role.Requests;
using VGS.RetailOS.Modules.Role.BL;
using VGS.RetailOS.Modules.Role.BO;
using VGS.RetailOS.Modules.Role.IDAC;
using VGS.RetailOS.Modules.User.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Role;

public class RoleBLTests
{
    private readonly Mock<IRoleDAC> _roleDacMock;
    private readonly Mock<IUserDAC> _userDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly RoleBL _sut;

    public RoleBLTests()
    {
        _roleDacMock = new Mock<IRoleDAC>();
        _userDacMock = new Mock<IUserDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new RoleBL(_roleDacMock.Object, _userDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldCreateRole_WhenValidRequest()
    {
        // Arrange
        var request = new CreateRoleRequest { Name = "Manager", Permissions = ["pos.sale"] };
        _roleDacMock.Setup(x => x.CreateRoleAsync(It.IsAny<RoleBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleBO r, CancellationToken c) => r);

        // Act
        var result = await _sut.CreateRoleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Manager", result.Name);
        Assert.Contains("pos.sale", result.Permissions);
        _roleDacMock.Verify(x => x.CreateRoleAsync(It.IsAny<RoleBO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_ShouldThrowConflictException_WhenUserNotInTenant()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _roleDacMock.Setup(x => x.GetRoleByIdAsync(roleId, "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleBO { Id = roleId, Name = "Role", TenantId = "tenant-1" });

        _userDacMock.Setup(x => x.IsUserInTenantAsync(userId, "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new AssignRoleRequest { UserId = userId };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _sut.AssignRoleToUserAsync(roleId, request));
        _roleDacMock.Verify(x => x.AssignRoleToUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_ShouldAssignRole_WhenUserInTenant()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _roleDacMock.Setup(x => x.GetRoleByIdAsync(roleId, "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleBO { Id = roleId, Name = "Role", TenantId = "tenant-1" });

        _userDacMock.Setup(x => x.IsUserInTenantAsync(userId, "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new AssignRoleRequest { UserId = userId };

        // Act
        await _sut.AssignRoleToUserAsync(roleId, request);

        // Assert
        _roleDacMock.Verify(x => x.AssignRoleToUserAsync(userId, roleId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
