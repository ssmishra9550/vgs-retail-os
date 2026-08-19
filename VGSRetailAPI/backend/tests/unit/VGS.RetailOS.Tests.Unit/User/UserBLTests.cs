using Moq;
using VGS.RetailOS.Contracts.V1.User.Requests;
using VGS.RetailOS.Modules.Auth.IBL;
using VGS.RetailOS.Modules.User.BL;
using VGS.RetailOS.Modules.User.BO;
using VGS.RetailOS.Modules.User.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.User;

public class UserBLTests
{
    private readonly Mock<IUserDAC> _userDacMock;
    private readonly Mock<IPasswordVerifier> _passwordVerifierMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly UserBL _sut;

    public UserBLTests()
    {
        _userDacMock = new Mock<IUserDAC>();
        _passwordVerifierMock = new Mock<IPasswordVerifier>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new UserBL(_userDacMock.Object, _passwordVerifierMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowValidationException_WhenEmailIsNull()
    {
        // Arrange
        var request = new CreateUserRequest { Email = null!, FirstName = "John", LastName = "Doe", Password = "password" };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateUserAsync(request));
    }

    [Fact]
    public async Task CreateUserAsync_ShouldAddExistingUserToTenant_WhenUserAlreadyExistsGlobally()
    {
        // Arrange
        var request = new CreateUserRequest { Email = "test@example.com", FirstName = "John", LastName = "Doe", Password = "password" };
        var existingUser = new UserBO { Id = Guid.NewGuid(), Email = "test@example.com", FirstName = "John", LastName = "Doe" };
        
        _userDacMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        _userDacMock.Verify(x => x.AddUserToTenantAsync(existingUser.Id, "tenant-1", It.IsAny<CancellationToken>()), Times.Once);
        _userDacMock.Verify(x => x.CreateUserAsync(It.IsAny<UserBO>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new CreateUserRequest { Email = "test@example.com", FirstName = "John", LastName = "Doe", Password = "password" };
        
        _userDacMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserBO?)null);

        _passwordVerifierMock.Setup(x => x.HashPassword(request.Password)).Returns("hashed");

        _userDacMock.Setup(x => x.CreateUserAsync(It.IsAny<UserBO>(), "hashed", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserBO u, string p, CancellationToken c) => u);

        // Act
        var result = await _sut.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        _userDacMock.Verify(x => x.CreateUserAsync(It.IsAny<UserBO>(), "hashed", It.IsAny<CancellationToken>()), Times.Once);
        _userDacMock.Verify(x => x.AddUserToTenantAsync(It.IsAny<Guid>(), "tenant-1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
