using Moq;
using VGS.RetailOS.Contracts.V1.CustomerManagement.Requests;
using VGS.RetailOS.Modules.CustomerManagement.Customer.BL;
using VGS.RetailOS.Modules.CustomerManagement.Customer.BO;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.CustomerManagement;

public class CustomerBLTests
{
    private readonly Mock<ICustomerDAC> _customerDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly CustomerBL _sut;

    public CustomerBLTests()
    {
        _customerDacMock = new Mock<ICustomerDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new CustomerBL(_customerDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateCustomerAsync_ShouldThrowValidationException_WhenMobileExists()
    {
        // Arrange
        var request = new CreateCustomerRequest { FirstName = "John", Mobile = "1234567890" };
        _customerDacMock.Setup(x => x.GetCustomerByMobileAsync("1234567890", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerBO { Id = Guid.NewGuid(), Mobile = "1234567890" });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateCustomerAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateCustomerAsync_ShouldCreateCustomer_AndSetCreditBalanceToZero()
    {
        // Arrange
        var request = new CreateCustomerRequest { FirstName = "Jane", Mobile = "9876543210" };
        _customerDacMock.Setup(x => x.GetCustomerByMobileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerBO?)null);

        _customerDacMock.Setup(x => x.CreateCustomerAsync(It.IsAny<CustomerBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerBO b, CancellationToken c) => b);

        // Act
        var result = await _sut.CreateCustomerAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("9876543210", result.Mobile);
        Assert.Equal(0m, result.CreditBalance);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task UpdateCustomerAsync_ShouldThrowValidationException_WhenMobileBelongsToAnotherCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var anotherCustomerId = Guid.NewGuid();
        var request = new UpdateCustomerRequest { FirstName = "John", Mobile = "5555555555" };

        _customerDacMock.Setup(x => x.GetCustomerByIdAsync(customerId, "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerBO { Id = customerId, Mobile = "1111111111" });

        _customerDacMock.Setup(x => x.GetCustomerByMobileAsync("5555555555", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerBO { Id = anotherCustomerId, Mobile = "5555555555" }); // Found another customer with this mobile

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateCustomerAsync(customerId, request, CancellationToken.None));
    }
}
