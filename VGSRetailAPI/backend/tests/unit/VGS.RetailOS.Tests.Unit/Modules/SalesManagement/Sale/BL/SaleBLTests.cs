using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.SalesManagement.Requests;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IBL;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL;
using VGS.RetailOS.Modules.SalesManagement.Sale.BL;
using VGS.RetailOS.Modules.SalesManagement.Sale.BO;
using VGS.RetailOS.Modules.SalesManagement.Sale.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Modules.SalesManagement.Sale.BL;

public class SaleBLTests
{
    private readonly Mock<ISaleDAC> _saleDacMock;
    private readonly Mock<IInventoryBL> _inventoryBlMock;
    private readonly Mock<ICustomerBL> _customerBlMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly SaleBL _saleBl;

    private readonly string _tenantId = "tenant-123";
    private readonly Guid _storeId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public SaleBLTests()
    {
        _saleDacMock = new Mock<ISaleDAC>();
        _inventoryBlMock = new Mock<IInventoryBL>();
        _customerBlMock = new Mock<ICustomerBL>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();
        
        var tenantContext = new TenantContext(_tenantId);
        _tenantContextAccessorMock.Setup(m => m.TenantContext).Returns(tenantContext);

        _saleBl = new SaleBL(_saleDacMock.Object, _inventoryBlMock.Object, _customerBlMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateDraftSaleAsync_WithEmptyItems_ThrowsValidationException()
    {
        var request = new CreateSaleRequest
        {
            StoreId = _storeId,
            InvoiceNumber = "INV-001",
            SaleDate = DateTimeOffset.UtcNow,
            Items = new List<SaleItemRequest>()
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _saleBl.CreateDraftSaleAsync(request, CancellationToken.None));
        Assert.Equal("Sale must contain at least one item.", exception.Message);
    }

    [Fact]
    public async Task CreateDraftSaleAsync_PaidMoreThanGrandTotal_ThrowsValidationException()
    {
        var request = new CreateSaleRequest
        {
            StoreId = _storeId,
            InvoiceNumber = "INV-001",
            SaleDate = DateTimeOffset.UtcNow,
            TotalDiscount = 0,
            TotalTax = 0,
            PaidAmount = 150m, // More than Grand Total of 100
            Items = new List<SaleItemRequest>
            {
                new SaleItemRequest
                {
                    ProductId = _productId,
                    Quantity = 1,
                    UnitPrice = 100,
                    Discount = 0,
                    TaxAmount = 0
                }
            }
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _saleBl.CreateDraftSaleAsync(request, CancellationToken.None));
        Assert.Equal("Paid amount cannot exceed grand total.", exception.Message);
    }

    [Fact]
    public async Task CreateDraftSaleAsync_ValidRequest_CalculatesTotalsAndSaves()
    {
        var request = new CreateSaleRequest
        {
            StoreId = _storeId,
            InvoiceNumber = "INV-001",
            SaleDate = DateTimeOffset.UtcNow,
            TotalDiscount = 10m,
            TotalTax = 5m,
            PaidAmount = 965m,
            Items = new List<SaleItemRequest>
            {
                new SaleItemRequest
                {
                    ProductId = _productId,
                    Quantity = 10,
                    UnitPrice = 100, // SubTotal = 1000
                    Discount = 50,
                    TaxAmount = 20  // Line Total = 1000 - 50 + 20 = 970
                }
            }
        };

        var expectedGrandTotal = 1000m - 10m + 5m; // 995

        _saleDacMock.Setup(x => x.CreateDraftSaleAsync(It.IsAny<SaleBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SaleBO bo, CancellationToken _) =>
            {
                bo.Id = Guid.NewGuid();
                return bo;
            });

        var result = await _saleBl.CreateDraftSaleAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1000m, result.SubTotal);
        Assert.Equal(expectedGrandTotal, result.GrandTotal);
        _saleDacMock.Verify(x => x.CreateDraftSaleAsync(It.IsAny<SaleBO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteSaleAsync_CashSale_CallsInventoryButNotCustomer()
    {
        var saleId = Guid.NewGuid();
        var saleBo = new SaleBO
        {
            Id = saleId,
            TenantId = _tenantId,
            StoreId = _storeId,
            CustomerId = _customerId,
            InvoiceNumber = "INV-001",
            GrandTotal = 965m,
            PaidAmount = 965m, // Fully paid
            Status = "Draft",
            Items = new List<SaleItemBO>
            {
                new SaleItemBO { ProductId = _productId, Quantity = 10, Total = 970 }
            }
        };

        var completedSaleBo = new SaleBO
        {
            Id = saleId,
            TenantId = _tenantId,
            StoreId = _storeId,
            CustomerId = _customerId,
            InvoiceNumber = "INV-001",
            GrandTotal = 965m,
            PaidAmount = 965m,
            Status = "Completed",
            Items = new List<SaleItemBO>
            {
                new SaleItemBO { ProductId = _productId, Quantity = 10, Total = 970 }
            }
        };

        _saleDacMock.Setup(x => x.GetSaleByIdAsync(saleId, _tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(saleBo);
        _saleDacMock.Setup(x => x.CompleteSaleAsync(saleId, _tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(completedSaleBo);

        var result = await _saleBl.CompleteSaleAsync(saleId, CancellationToken.None);

        Assert.Equal("Completed", result.Status);

        // Verify Inventory is deducted (ChangeQuantity = -10)
        _inventoryBlMock.Verify(x => x.RecordTransactionAsync(It.Is<VGS.RetailOS.Contracts.V1.InventoryManagement.Requests.RecordStockTransactionRequest>(l => 
            l.ProductId == _productId &&
            l.ChangeQuantity == -10 &&
            l.TransactionType == "Sale" &&
            l.ReferenceId == saleId), It.IsAny<CancellationToken>()), Times.Once);

        // Verify Customer balance is NOT updated because it's fully paid
        _customerBlMock.Verify(x => x.UpdateCreditBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CompleteSaleAsync_CreditSale_CallsInventoryAndCustomer()
    {
        var saleId = Guid.NewGuid();
        var saleBo = new SaleBO
        {
            Id = saleId,
            TenantId = _tenantId,
            StoreId = _storeId,
            CustomerId = _customerId,
            InvoiceNumber = "INV-001",
            GrandTotal = 1000m,
            PaidAmount = 200m, // 800 Credit
            Status = "Draft",
            Items = new List<SaleItemBO>
            {
                new SaleItemBO { ProductId = _productId, Quantity = 5, Total = 1000 }
            }
        };

        var completedSaleBo = new SaleBO
        {
            Id = saleId,
            TenantId = _tenantId,
            StoreId = _storeId,
            CustomerId = _customerId,
            InvoiceNumber = "INV-001",
            GrandTotal = 1000m,
            PaidAmount = 200m,
            Status = "Completed",
            Items = new List<SaleItemBO>
            {
                new SaleItemBO { ProductId = _productId, Quantity = 5, Total = 1000 }
            }
        };

        _saleDacMock.Setup(x => x.GetSaleByIdAsync(saleId, _tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(saleBo);
        _saleDacMock.Setup(x => x.CompleteSaleAsync(saleId, _tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(completedSaleBo);

        var result = await _saleBl.CompleteSaleAsync(saleId, CancellationToken.None);

        Assert.Equal("Completed", result.Status);

        _inventoryBlMock.Verify(x => x.RecordTransactionAsync(It.Is<VGS.RetailOS.Contracts.V1.InventoryManagement.Requests.RecordStockTransactionRequest>(l => 
            l.ProductId == _productId &&
            l.ChangeQuantity == -5 &&
            l.TransactionType == "Sale"), It.IsAny<CancellationToken>()), Times.Once);

        // Verify Customer balance is increased by 800
        _customerBlMock.Verify(x => x.UpdateCreditBalanceAsync(_customerId, 800m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelSaleAsync_CallsDac()
    {
        var saleId = Guid.NewGuid();
        var saleBo = new SaleBO
        {
            Id = saleId,
            TenantId = _tenantId,
            Status = "Cancelled"
        };

        _saleDacMock.Setup(x => x.CancelSaleAsync(saleId, _tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(saleBo);

        var result = await _saleBl.CancelSaleAsync(saleId, CancellationToken.None);

        Assert.Equal("Cancelled", result.Status);
        _saleDacMock.Verify(x => x.CancelSaleAsync(saleId, _tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReturnAsync_UpdatesInventoryAndCustomerCredit()
    {
        var saleId = Guid.NewGuid();
        var request = new ProcessReturnRequest();

        var returnedSaleBo = new SaleBO
        {
            Id = saleId,
            TenantId = _tenantId,
            StoreId = _storeId,
            CustomerId = _customerId,
            InvoiceNumber = "INV-001",
            GrandTotal = 1000m,
            PaidAmount = 200m, // 800 originally on credit
            Status = "Returned",
            Items = new List<SaleItemBO>
            {
                new SaleItemBO { ProductId = _productId, Quantity = 5, Total = 1000 }
            }
        };

        _saleDacMock.Setup(x => x.ReturnSaleAsync(saleId, _tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(returnedSaleBo);

        var result = await _saleBl.ProcessReturnAsync(saleId, request, CancellationToken.None);

        Assert.Equal("Returned", result.Status);
        _saleDacMock.Verify(x => x.ReturnSaleAsync(saleId, _tenantId, It.IsAny<CancellationToken>()), Times.Once);

        // Verify Inventory is added back (ChangeQuantity = +5)
        _inventoryBlMock.Verify(x => x.RecordTransactionAsync(It.Is<VGS.RetailOS.Contracts.V1.InventoryManagement.Requests.RecordStockTransactionRequest>(l => 
            l.ProductId == _productId &&
            l.ChangeQuantity == 5 &&
            l.TransactionType == "SalesReturn"), It.IsAny<CancellationToken>()), Times.Once);

        // Verify Customer balance is decreased by 800 (undoing the original debt)
        _customerBlMock.Verify(x => x.UpdateCreditBalanceAsync(_customerId, -800m, It.IsAny<CancellationToken>()), Times.Once);
    }
}
