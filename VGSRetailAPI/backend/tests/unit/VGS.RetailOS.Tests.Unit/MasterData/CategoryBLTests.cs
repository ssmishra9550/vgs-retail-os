using Moq;
using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Modules.MasterData.Category.BL;
using VGS.RetailOS.Modules.MasterData.Category.BO;
using VGS.RetailOS.Modules.MasterData.Category.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.MasterData;

public class CategoryBLTests
{
    private readonly Mock<ICategoryDAC> _categoryDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly CategoryBL _sut;

    public CategoryBLTests()
    {
        _categoryDacMock = new Mock<ICategoryDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new CategoryBL(_categoryDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldThrowValidationException_WhenNameExists()
    {
        // Arrange
        var request = new CreateCategoryRequest { Name = "ExistingCategory" };
        _categoryDacMock.Setup(x => x.GetCategoryByNameAsync("ExistingCategory", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CategoryBO { Id = Guid.NewGuid(), Name = "ExistingCategory" });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateCategoryAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest { Name = "NewCategory", Description = "Desc" };
        _categoryDacMock.Setup(x => x.GetCategoryByNameAsync("NewCategory", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryBO?)null);

        _categoryDacMock.Setup(x => x.CreateCategoryAsync(It.IsAny<CategoryBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryBO b, CancellationToken c) => b);

        // Act
        var result = await _sut.CreateCategoryAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NewCategory", result.Name);
        Assert.True(result.IsActive);
    }
}
