using Moq;
using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Modules.MasterData.Brand.BL;
using VGS.RetailOS.Modules.MasterData.Brand.BO;
using VGS.RetailOS.Modules.MasterData.Brand.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.MasterData;

public class BrandBLTests
{
    private readonly Mock<IBrandDAC> _brandDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly BrandBL _sut;

    public BrandBLTests()
    {
        _brandDacMock = new Mock<IBrandDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new BrandBL(_brandDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateBrandAsync_ShouldThrowValidationException_WhenNameExists()
    {
        // Arrange
        var request = new CreateBrandRequest { Name = "ExistingBrand" };
        _brandDacMock.Setup(x => x.GetBrandByNameAsync("ExistingBrand", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrandBO { Id = Guid.NewGuid(), Name = "ExistingBrand" });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateBrandAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateBrandAsync_ShouldCreateBrand()
    {
        // Arrange
        var request = new CreateBrandRequest { Name = "NewBrand", Description = "Desc" };
        _brandDacMock.Setup(x => x.GetBrandByNameAsync("NewBrand", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((BrandBO?)null);

        _brandDacMock.Setup(x => x.CreateBrandAsync(It.IsAny<BrandBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BrandBO b, CancellationToken c) => b);

        // Act
        var result = await _sut.CreateBrandAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NewBrand", result.Name);
        Assert.True(result.IsActive);
    }
}
