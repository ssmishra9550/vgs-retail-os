using Moq;
using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.BL;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.BO;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Tests.Unit.Modules.InventoryManagement.Inventory.BL;

public class InventoryBLTests
{
    private readonly Mock<IInventoryDAC> _inventoryDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly InventoryBL _inventoryBl;
    
    private readonly string _tenantId = "tenant-1";

    public InventoryBLTests()
    {
        _inventoryDacMock = new Mock<IInventoryDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();
        
        var tenantContext = new TenantContext(_tenantId);
        _tenantContextAccessorMock.Setup(m => m.TenantContext).Returns(tenantContext);

        _inventoryBl = new InventoryBL(_inventoryDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task RecordTransactionAsync_ShouldThrowValidationException_WhenChangeQuantityIsZero()
    {
        // Arrange
        var request = new RecordStockTransactionRequest
        {
            StoreId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ChangeQuantity = 0,
            TransactionType = "Adjustment",
            ReferenceId = Guid.NewGuid()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _inventoryBl.RecordTransactionAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RecordTransactionAsync_ShouldRecordTransactionAndReturnResponse_WhenValidRequest()
    {
        // Arrange
        var request = new RecordStockTransactionRequest
        {
            StoreId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ChangeQuantity = 10,
            TransactionType = "Purchase",
            ReferenceId = Guid.NewGuid(),
            Reason = "New Stock"
        };

        var returnedBo = new InventoryLedgerBO
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            StoreId = request.StoreId,
            ProductId = request.ProductId,
            ChangeQuantity = request.ChangeQuantity,
            BalanceAfter = 15,
            TransactionType = request.TransactionType,
            ReferenceId = request.ReferenceId,
            Reason = request.Reason,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _inventoryDacMock.Setup(m => m.RecordTransactionAsync(It.IsAny<InventoryLedgerBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(returnedBo);

        // Act
        var result = await _inventoryBl.RecordTransactionAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(returnedBo.Id, result.Id);
        Assert.Equal(returnedBo.BalanceAfter, result.BalanceAfter);
        Assert.Equal(returnedBo.ChangeQuantity, result.ChangeQuantity);
        
        _inventoryDacMock.Verify(m => m.RecordTransactionAsync(
            It.Is<InventoryLedgerBO>(bo => 
                bo.TenantId == _tenantId &&
                bo.StoreId == request.StoreId &&
                bo.ProductId == request.ProductId &&
                bo.ChangeQuantity == request.ChangeQuantity &&
                bo.TransactionType == request.TransactionType), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStockBalanceAsync_ShouldReturnNull_WhenNoBalanceExists()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _inventoryDacMock.Setup(m => m.GetStockBalanceAsync(_tenantId, storeId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockBalanceBO?)null);

        // Act
        var result = await _inventoryBl.GetStockBalanceAsync(storeId, productId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStockHistoryAsync_ShouldReturnMappedResponses()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var history = new List<InventoryLedgerBO>
        {
            new InventoryLedgerBO { Id = Guid.NewGuid(), TransactionType = "Purchase", ChangeQuantity = 10, BalanceAfter = 10 },
            new InventoryLedgerBO { Id = Guid.NewGuid(), TransactionType = "Sale", ChangeQuantity = -2, BalanceAfter = 8 }
        };

        _inventoryDacMock.Setup(m => m.GetStockHistoryAsync(_tenantId, storeId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _inventoryBl.GetStockHistoryAsync(storeId, productId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Purchase", result[0].TransactionType);
        Assert.Equal("Sale", result[1].TransactionType);
    }
}
