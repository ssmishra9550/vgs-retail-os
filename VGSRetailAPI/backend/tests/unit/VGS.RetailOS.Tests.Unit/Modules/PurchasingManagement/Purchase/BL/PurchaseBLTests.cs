using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.PurchasingManagement.Requests;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.BO;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.BL;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.BO;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.IDAC;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IBL;
using VGS.RetailOS.Shared.Tenancy;
using VGS.RetailOS.Shared.Errors.Exceptions;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Modules.PurchasingManagement.Purchase.BL;

public class PurchaseBLTests
{
    private readonly Mock<IPurchaseDAC> _purchaseDacMock;
    private readonly Mock<IInventoryBL> _inventoryBlMock;
    private readonly Mock<ISupplierBL> _supplierBlMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly PurchaseBL _purchaseBl;

    private readonly string _tenantId = "tenant-123";
    private readonly Guid _storeId = Guid.NewGuid();
    private readonly Guid _supplierId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public PurchaseBLTests()
    {
        _purchaseDacMock = new Mock<IPurchaseDAC>();
        _inventoryBlMock = new Mock<IInventoryBL>();
        _supplierBlMock = new Mock<ISupplierBL>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();
        
        var tenantContext = new TenantContext(_tenantId);
        _tenantContextAccessorMock.Setup(m => m.TenantContext).Returns(tenantContext);

        _purchaseBl = new PurchaseBL(_purchaseDacMock.Object, _inventoryBlMock.Object, _supplierBlMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateDraftPurchaseAsync_WithEmptyItems_ThrowsValidationException()
    {
        var request = new CreatePurchaseRequest
        {
            StoreId = _storeId,
            SupplierId = _supplierId,
            InvoiceNumber = "INV-001",
            InvoiceDate = DateTimeOffset.UtcNow,
            Items = new List<PurchaseItemRequest>()
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _purchaseBl.CreateDraftPurchaseAsync(request, CancellationToken.None));
        Assert.Equal("Purchase must contain at least one item.", exception.Message);
    }

    [Fact]
    public async Task CreateDraftPurchaseAsync_ValidRequest_CalculatesTotalsAndSaves()
    {
        var request = new CreatePurchaseRequest
        {
            StoreId = _storeId,
            SupplierId = _supplierId,
            InvoiceNumber = "INV-001",
            InvoiceDate = DateTimeOffset.UtcNow,
            TotalDiscount = 10m,
            TotalTax = 5m,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductId = _productId,
                    Quantity = 10,
                    UnitCost = 100, // SubTotal = 1000
                    Discount = 50,
                    TaxAmount = 20  // Line Total = 1000 - 50 + 20 = 970
                }
            }
        };

        var expectedGrandTotal = 970m - 10m + 5m; // 965

        _purchaseDacMock.Setup(x => x.CreateDraftPurchaseAsync(It.IsAny<PurchaseBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseBO bo, CancellationToken _) =>
            {
                bo.Id = Guid.NewGuid();
                return bo;
            });

        var result = await _purchaseBl.CreateDraftPurchaseAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1000m, result.SubTotal);
        Assert.Equal(expectedGrandTotal, result.GrandTotal);
        _purchaseDacMock.Verify(x => x.CreateDraftPurchaseAsync(It.IsAny<PurchaseBO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReceivePurchaseAsync_ValidPurchase_CallsInventoryAndSupplierBL()
    {
        var purchaseId = Guid.NewGuid();
        var purchaseBo = new PurchaseBO
        {
            Id = purchaseId,
            TenantId = _tenantId,
            StoreId = _storeId,
            SupplierId = _supplierId,
            InvoiceNumber = "INV-001",
            GrandTotal = 965m,
            Status = "Received",
            Items = new List<PurchaseItemBO>
            {
                new PurchaseItemBO { ProductId = _productId, Quantity = 10, Total = 970 }
            }
        };

        _purchaseDacMock.Setup(x => x.MarkAsReceivedAsync(purchaseId, _tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseBo);

        var result = await _purchaseBl.ReceivePurchaseAsync(purchaseId, CancellationToken.None);

        Assert.Equal("Received", result.Status);

        _inventoryBlMock.Verify(x => x.RecordTransactionAsync(It.Is<VGS.RetailOS.Contracts.V1.InventoryManagement.Requests.RecordStockTransactionRequest>(l => 
            l.ProductId == _productId &&
            l.ChangeQuantity == 10 &&
            l.TransactionType == "PurchaseReceipt" &&
            l.ReferenceId == purchaseId), It.IsAny<CancellationToken>()), Times.Once);

        _supplierBlMock.Verify(x => x.UpdateOutstandingPayableAsync(_supplierId, 965m, It.IsAny<CancellationToken>()), Times.Once);
    }
}
