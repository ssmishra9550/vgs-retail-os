using VGS.RetailOS.Contracts.V1.SalesManagement.Requests;
using VGS.RetailOS.Contracts.V1.SalesManagement.Responses;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IBL;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL;
using VGS.RetailOS.Modules.SalesManagement.Sale.BO;
using VGS.RetailOS.Modules.SalesManagement.Sale.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Modules.SalesManagement.Sale.IBL;

namespace VGS.RetailOS.Modules.SalesManagement.Sale.BL;

public class SaleBL : ISaleBL
{
    private readonly ISaleDAC _saleDac;
    private readonly IInventoryBL _inventoryBl;
    private readonly ICustomerBL _customerBl;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public SaleBL(ISaleDAC saleDac, IInventoryBL inventoryBl, ICustomerBL customerBl, ITenantContextAccessor tenantContextAccessor)
    {
        _saleDac = saleDac;
        _inventoryBl = inventoryBl;
        _customerBl = customerBl;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<SaleResponse> CreateDraftSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        if (request.Items == null || !request.Items.Any())
            throw new ValidationException("Sale must contain at least one item.");

        decimal subTotal = 0;
        decimal grandTotal = 0;

        var items = new List<SaleItemBO>();
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0) throw new ValidationException("Item quantity must be greater than zero.");
            
            var lineSubTotal = item.Quantity * item.UnitPrice;
            var lineTotal = lineSubTotal - item.Discount + item.TaxAmount;

            subTotal += lineSubTotal;
            
            items.Add(new SaleItemBO
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount,
                TaxAmount = item.TaxAmount,
                Total = lineTotal
            });
        }

        grandTotal = subTotal - request.TotalDiscount + request.TotalTax;

        if (request.PaidAmount > grandTotal)
            throw new ValidationException("Paid amount cannot exceed grand total.");

        var saleBo = new SaleBO
        {
            TenantId = tenantId,
            StoreId = request.StoreId,
            CustomerId = request.CustomerId,
            InvoiceNumber = request.InvoiceNumber,
            SaleDate = request.SaleDate,
            SubTotal = subTotal,
            TotalDiscount = request.TotalDiscount,
            TotalTax = request.TotalTax,
            GrandTotal = grandTotal,
            PaidAmount = request.PaidAmount,
            Items = items
        };

        var createdSale = await _saleDac.CreateDraftSaleAsync(saleBo, cancellationToken);
        return MapToResponse(createdSale);
    }

    public async Task<SaleResponse?> GetSaleByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var sale = await _saleDac.GetSaleByIdAsync(id, tenantId, cancellationToken);
        return sale == null ? null : MapToResponse(sale);
    }

    public async Task<SaleResponse> CompleteSaleAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        
        var sale = await _saleDac.GetSaleByIdAsync(id, tenantId, cancellationToken);
        if (sale == null)
            throw new NotFoundException("Sale not found.");

        if (sale.Status != "Draft")
            throw new ValidationException($"Sale cannot be completed from status '{sale.Status}'.");

        // 1. Mark Sale as Completed
        var completedSale = await _saleDac.CompleteSaleAsync(id, tenantId, cancellationToken);

        // 2. Decrease Inventory for each item
        foreach (var item in completedSale.Items)
        {
            var stockRequest = new RecordStockTransactionRequest
            {
                StoreId = completedSale.StoreId,
                ProductId = item.ProductId,
                ChangeQuantity = -item.Quantity, // Deduct stock
                TransactionType = "Sale",
                ReferenceId = completedSale.Id,
                Reason = $"Sold on invoice {completedSale.InvoiceNumber}"
            };
            
            await _inventoryBl.RecordTransactionAsync(stockRequest, cancellationToken);
        }

        // 3. If credit sale (PaidAmount < GrandTotal), increase customer credit balance
        if (completedSale.PaidAmount < completedSale.GrandTotal)
        {
            if (!completedSale.CustomerId.HasValue)
                throw new ValidationException("Credit sales require a valid Customer ID.");

            var creditAmount = completedSale.GrandTotal - completedSale.PaidAmount;
            await _customerBl.UpdateCreditBalanceAsync(completedSale.CustomerId.Value, creditAmount, cancellationToken);
        }

        return MapToResponse(completedSale);
    }

    public async Task<IEnumerable<SaleResponse>> GetDraftSalesAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var sales = await _saleDac.GetDraftSalesAsync(storeId, tenantId, cancellationToken);
        return sales.Select(MapToResponse);
    }

    public async Task<IEnumerable<SaleResponse>> GetSalesHistoryAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var sales = await _saleDac.GetSalesHistoryAsync(storeId, tenantId, cancellationToken);
        return sales.Select(MapToResponse);
    }

    public async Task<SaleResponse> CancelSaleAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var sale = await _saleDac.CancelSaleAsync(id, tenantId, cancellationToken);
        return MapToResponse(sale);
    }

    public async Task<SaleResponse> ProcessReturnAsync(Guid id, ProcessReturnRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        
        // 1. Mark Sale as Returned
        var returnedSale = await _saleDac.ReturnSaleAsync(id, tenantId, cancellationToken);

        // 2. Increase Inventory for each returned item
        // In MVP, we return the entire invoice. We should validate the request items match, but for simplicity, we process all items.
        // Actually, we'll iterate through the returned items in the invoice and put them back in stock.
        foreach (var item in returnedSale.Items)
        {
            var stockRequest = new RecordStockTransactionRequest
            {
                StoreId = returnedSale.StoreId,
                ProductId = item.ProductId,
                ChangeQuantity = item.Quantity, // Add stock back
                TransactionType = "SalesReturn",
                ReferenceId = returnedSale.Id,
                Reason = $"Return on invoice {returnedSale.InvoiceNumber}"
            };
            
            await _inventoryBl.RecordTransactionAsync(stockRequest, cancellationToken);
        }

        // 3. If it was a credit sale, reduce the customer's credit balance
        if (returnedSale.PaidAmount < returnedSale.GrandTotal)
        {
            var creditAmount = returnedSale.GrandTotal - returnedSale.PaidAmount;
            
            // Wait! The customer owes us `creditAmount`. If they return it, we must reduce their debt by `creditAmount`.
            // UpdateCreditBalanceAsync takes negative to reduce debt. So we add back the debt to reduce it?
            // Actually, in `CompleteSaleAsync`, we did: 
            // `await _customerBl.UpdateCreditBalanceAsync(..., creditAmount, ...)` which INCREASED the debt.
            // So to reverse it, we do `-creditAmount`.
            if (returnedSale.CustomerId.HasValue)
            {
                await _customerBl.UpdateCreditBalanceAsync(returnedSale.CustomerId.Value, -creditAmount, cancellationToken);
            }
        }
        
        // What about PaidAmount? If they paid cash, we physically give them back the cash. We assume the cashier does this.
        // We might want to integrate this with Payments module to log a negative payment, but for MVP, stock and credit reversal is enough.

        return MapToResponse(returnedSale);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    private static SaleResponse MapToResponse(SaleBO bo)
    {
        return new SaleResponse
        {
            Id = bo.Id,
            StoreId = bo.StoreId,
            CustomerId = bo.CustomerId,
            InvoiceNumber = bo.InvoiceNumber,
            SaleDate = bo.SaleDate,
            Status = bo.Status,
            SubTotal = bo.SubTotal,
            TotalDiscount = bo.TotalDiscount,
            TotalTax = bo.TotalTax,
            GrandTotal = bo.GrandTotal,
            PaidAmount = bo.PaidAmount,
            Items = bo.Items.Select(i => new SaleItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TaxAmount = i.TaxAmount,
                Total = i.Total
            }).ToList()
        };
    }
}
