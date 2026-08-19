using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.SalesManagement.DAC.Entities;
using VGS.RetailOS.Modules.SalesManagement.Sale.BO;
using VGS.RetailOS.Modules.SalesManagement.Sale.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;

namespace VGS.RetailOS.Infrastructure.SalesManagement.DAC;

public class SaleDAC : ISaleDAC
{
    private readonly AppDbContext _dbContext;

    public SaleDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SaleBO> CreateDraftSaleAsync(SaleBO sale, CancellationToken cancellationToken)
    {
        var existingInvoice = await _dbContext.Sales
            .FirstOrDefaultAsync(s => s.TenantId == sale.TenantId && s.InvoiceNumber == sale.InvoiceNumber, cancellationToken);
            
        if (existingInvoice != null)
            throw new ValidationException($"Invoice Number '{sale.InvoiceNumber}' already exists.");

        var entity = new SaleEntity
        {
            TenantId = sale.TenantId,
            StoreId = sale.StoreId,
            CustomerId = sale.CustomerId,
            InvoiceNumber = sale.InvoiceNumber,
            SaleDate = sale.SaleDate,
            Status = "Draft",
            SubTotal = sale.SubTotal,
            TotalDiscount = sale.TotalDiscount,
            TotalTax = sale.TotalTax,
            GrandTotal = sale.GrandTotal,
            PaidAmount = sale.PaidAmount,
            Items = sale.Items.Select(i => new SaleItemEntity
            {
                TenantId = sale.TenantId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TaxAmount = i.TaxAmount,
                Total = i.Total
            }).ToList()
        };

        _dbContext.Sales.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetSaleByIdAsync(entity.Id, entity.TenantId, cancellationToken) 
               ?? throw new ValidationException("Failed to retrieve created sale.");
    }

    public async Task<SaleBO?> GetSaleByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);

        if (entity == null) return null;

        return MapToBO(entity);
    }

    public async Task<SaleBO> CompleteSaleAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);

        if (entity == null)
            throw new NotFoundException("Sale not found.");

        if (entity.Status != "Draft")
            throw new ValidationException($"Sale cannot be completed from status '{entity.Status}'.");

        entity.Status = "Completed";
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToBO(entity);
    }

    public async Task<IEnumerable<SaleBO>> GetDraftSalesAsync(Guid storeId, string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Sales
            .Include(s => s.Items)
            .Where(s => s.StoreId == storeId && s.TenantId == tenantId && s.Status == "Draft")
            .ToListAsync(cancellationToken);

        return entities.Select(MapToBO);
    }

    public async Task<IEnumerable<SaleBO>> GetSalesHistoryAsync(Guid storeId, string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Sales
            .Include(s => s.Items)
            .Where(s => s.StoreId == storeId && s.TenantId == tenantId && s.Status != "Draft")
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToBO);
    }

    public async Task<SaleBO> CancelSaleAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);

        if (entity == null)
            throw new NotFoundException("Sale not found.");

        if (entity.Status != "Draft")
            throw new ValidationException($"Sale cannot be cancelled from status '{entity.Status}'.");

        entity.Status = "Cancelled";
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToBO(entity);
    }

    public async Task<SaleBO> ReturnSaleAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);

        if (entity == null)
            throw new NotFoundException("Sale not found.");

        if (entity.Status != "Completed")
            throw new ValidationException($"Only completed sales can be returned.");

        entity.Status = "Returned";
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToBO(entity);
    }

    private static SaleBO MapToBO(SaleEntity entity)
    {
        return new SaleBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            StoreId = entity.StoreId,
            CustomerId = entity.CustomerId,
            InvoiceNumber = entity.InvoiceNumber,
            SaleDate = entity.SaleDate,
            Status = entity.Status,
            SubTotal = entity.SubTotal,
            TotalDiscount = entity.TotalDiscount,
            TotalTax = entity.TotalTax,
            GrandTotal = entity.GrandTotal,
            PaidAmount = entity.PaidAmount,
            Items = entity.Items.Select(i => new SaleItemBO
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
