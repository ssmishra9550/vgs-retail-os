using Moq;
using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Modules.MasterData.Unit.BL;
using VGS.RetailOS.Modules.MasterData.Unit.BO;
using VGS.RetailOS.Modules.MasterData.Unit.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.MasterData;

public class UnitBLTests
{
    private readonly Mock<IUnitDAC> _unitDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly UnitBL _sut;

    public UnitBLTests()
    {
        _unitDacMock = new Mock<IUnitDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new UnitBL(_unitDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateUnitAsync_ShouldThrowValidationException_WhenNameExists()
    {
        // Arrange
        var request = new CreateUnitRequest { Name = "ExistingUnit", ShortName = "EU" };
        _unitDacMock.Setup(x => x.GetUnitByNameAsync("ExistingUnit", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UnitBO { Id = Guid.NewGuid(), Name = "ExistingUnit" });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateUnitAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateUnitAsync_ShouldCreateUnit()
    {
        // Arrange
        var request = new CreateUnitRequest { Name = "Kilogram", ShortName = "kg" };
        _unitDacMock.Setup(x => x.GetUnitByNameAsync("Kilogram", "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UnitBO?)null);

        _unitDacMock.Setup(x => x.CreateUnitAsync(It.IsAny<UnitBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UnitBO u, CancellationToken c) => u);

        // Act
        var result = await _sut.CreateUnitAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Kilogram", result.Name);
        Assert.Equal("kg", result.ShortName);
    }
}
