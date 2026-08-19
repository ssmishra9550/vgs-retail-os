using Moq;
using VGS.RetailOS.Contracts.V1.ProductManagement.Requests;
using VGS.RetailOS.Modules.ProductManagement.Product.BL;
using VGS.RetailOS.Modules.ProductManagement.Product.BO;
using VGS.RetailOS.Modules.ProductManagement.Product.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.ProductManagement;

public class ProductBLTests
{
    private readonly Mock<IProductDAC> _productDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly ProductBL _sut;

    public ProductBLTests()
    {
        _productDacMock = new Mock<IProductDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new ProductBL(_productDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrowValidationException_WhenSkuExists()
    {
        // Arrange
        var request = new CreateProductRequest { Name = "NewProduct", Sku = "SKU-123" };
        _productDacMock.Setup(x => x.GetProductBySkuAsync("SKU-123", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductBO { Id = Guid.NewGuid(), Sku = "SKU-123" });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateProductAsync(request, CancellationToken.None));
    }



    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct()
    {
        // Arrange
        var request = new CreateProductRequest { Name = "Smartphone", PurchasePrice = 500m, SellingPrice = 800m, UnitId = Guid.NewGuid() };
        _productDacMock.Setup(x => x.GetProductBySkuAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductBO?)null);

        _productDacMock.Setup(x => x.CreateProductAsync(It.IsAny<ProductBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductBO b, CancellationToken c) => b);

        // Act
        var result = await _sut.CreateProductAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Smartphone", result.Name);
        Assert.Equal(500m, result.PurchasePrice);
        Assert.Equal(800m, result.SellingPrice);
    }
}
