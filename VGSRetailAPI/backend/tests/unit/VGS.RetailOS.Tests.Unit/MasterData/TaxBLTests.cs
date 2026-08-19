using Moq;
using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Modules.MasterData.Tax.BL;
using VGS.RetailOS.Modules.MasterData.Tax.BO;
using VGS.RetailOS.Modules.MasterData.Tax.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.MasterData;

public class TaxBLTests
{
    private readonly Mock<ITaxDAC> _taxDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly TaxBL _sut;

    public TaxBLTests()
    {
        _taxDacMock = new Mock<ITaxDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new TaxBL(_taxDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateTaxAsync_ShouldThrowValidationException_WhenInvalidType()
    {
        // Arrange
        var request = new CreateTaxRequest { Name = "NewTax", Rate = 10, Type = "InvalidType" };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateTaxAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateTaxAsync_ShouldCreateTax()
    {
        // Arrange
        var request = new CreateTaxRequest { Name = "GST", Rate = 18.0m, Type = "Percentage" };
        _taxDacMock.Setup(x => x.GetTaxByNameAsync("GST", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxBO?)null);

        _taxDacMock.Setup(x => x.CreateTaxAsync(It.IsAny<TaxBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxBO t, CancellationToken c) => t);

        // Act
        var result = await _sut.CreateTaxAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GST", result.Name);
        Assert.Equal(18.0m, result.Rate);
    }
}
