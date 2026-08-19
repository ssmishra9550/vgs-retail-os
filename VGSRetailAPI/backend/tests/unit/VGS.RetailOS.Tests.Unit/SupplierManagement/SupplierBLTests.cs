using Moq;
using VGS.RetailOS.Contracts.V1.SupplierManagement.Requests;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.BL;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.BO;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.SupplierManagement;

public class SupplierBLTests
{
    private readonly Mock<ISupplierDAC> _supplierDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly SupplierBL _sut;

    public SupplierBLTests()
    {
        _supplierDacMock = new Mock<ISupplierDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new SupplierBL(_supplierDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateSupplierAsync_ShouldThrowValidationException_WhenNameExists()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = "Acme Corp", Mobile = "1234567890" };

        _supplierDacMock.Setup(x => x.ExistsByNameAsync("Acme Corp", "tenant-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateSupplierAsync(request, CancellationToken.None));
        Assert.Contains("name 'Acme Corp' already exists", exception.Message);
    }

    [Fact]
    public async Task CreateSupplierAsync_ShouldThrowValidationException_WhenMobileExists()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = "Acme Corp", Mobile = "1234567890" };

        _supplierDacMock.Setup(x => x.ExistsByNameAsync("Acme Corp", "tenant-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _supplierDacMock.Setup(x => x.ExistsByMobileAsync("1234567890", "tenant-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateSupplierAsync(request, CancellationToken.None));
        Assert.Contains("mobile '1234567890' already exists", exception.Message);
    }

    [Fact]
    public async Task CreateSupplierAsync_ShouldCreateSupplier_WhenValid()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = "Acme Corp", Mobile = "1234567890" };

        _supplierDacMock.Setup(x => x.ExistsByNameAsync("Acme Corp", "tenant-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _supplierDacMock.Setup(x => x.ExistsByMobileAsync("1234567890", "tenant-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _supplierDacMock.Setup(x => x.CreateSupplierAsync(It.IsAny<SupplierBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupplierBO bo, CancellationToken ct) => bo);

        // Act
        var result = await _sut.CreateSupplierAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Acme Corp", result.Name);
        Assert.Equal("1234567890", result.Mobile);
        Assert.Equal(0, result.OutstandingPayable);
    }

    [Fact]
    public async Task UpdateSupplierAsync_ShouldThrowValidationException_WhenNameExistsForAnotherSupplier()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateSupplierRequest { Id = id, Name = "Acme Corp", Mobile = "1234567890" };

        _supplierDacMock.Setup(x => x.ExistsByNameAsync("Acme Corp", "tenant-1", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateSupplierAsync(request, CancellationToken.None));
        Assert.Contains("name 'Acme Corp' already exists", exception.Message);
    }
}
