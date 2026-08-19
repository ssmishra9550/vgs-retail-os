using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.PurchasingManagement.DAC.Entities;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.BO;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;

namespace VGS.RetailOS.Infrastructure.PurchasingManagement.DAC;

public class PurchaseDAC : IPurchaseDAC
{
    private readonly AppDbContext _dbContext;

    public PurchaseDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PurchaseBO> CreateDraftPurchaseAsync(PurchaseBO purchase, CancellationToken cancellationToken)
    {
        var entity = new PurchaseEntity
        {
            Id = purchase.Id == Guid.Empty ? Guid.NewGuid() : purchase.Id,
            TenantId = purchase.TenantId,
            StoreId = purchase.StoreId,
            SupplierId = purchase.SupplierId,
            InvoiceNumber = purchase.InvoiceNumber,
            InvoiceDate = purchase.InvoiceDate,
            Status = "Draft",
            SubTotal = purchase.SubTotal,
            TotalDiscount = purchase.TotalDiscount,
            TotalTax = purchase.TotalTax,
            GrandTotal = purchase.GrandTotal,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = purchase.Items.Select(i => new PurchaseItemEntity
            {
                Id = Guid.NewGuid(),
                TenantId = purchase.TenantId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Discount = i.Discount,
                TaxAmount = i.TaxAmount,
                Total = i.Total
            }).ToList()
        };

        try
        {
            _dbContext.Purchases.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505")
            {
                throw new ValidationException("A purchase with this Invoice Number already exists for the selected Supplier.");
            }
            throw;
        }

        return await GetPurchaseByIdAsync(entity.Id, entity.TenantId, cancellationToken) 
               ?? throw new ValidationException("Failed to retrieve created purchase.");
    }

    public async Task<PurchaseBO?> GetPurchaseByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Purchases
            .Include(p => p.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);

        if (entity == null) return null;

        return MapToBO(entity);
    }

    public async Task<IEnumerable<PurchaseBO>> GetAllPurchasesAsync(Guid storeId, string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Purchases
            .Include(p => p.Items)
            .AsNoTracking()
            .Where(p => p.StoreId == storeId && p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToBO);
    }

    private static PurchaseBO MapToBO(PurchaseEntity entity)
    {
        return new PurchaseBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            StoreId = entity.StoreId,
            SupplierId = entity.SupplierId,
            InvoiceNumber = entity.InvoiceNumber,
            InvoiceDate = entity.InvoiceDate,
            Status = entity.Status,
            SubTotal = entity.SubTotal,
            TotalDiscount = entity.TotalDiscount,
            TotalTax = entity.TotalTax,
            GrandTotal = entity.GrandTotal,
            CreatedAt = entity.CreatedAt,
            Items = entity.Items.Select(i => new PurchaseItemBO
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Discount = i.Discount,
                TaxAmount = i.TaxAmount,
                Total = i.Total
            }).ToList()
        };
    }

    public async Task<PurchaseBO> MarkAsReceivedAsync(Guid purchaseId, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Purchases
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == purchaseId && p.TenantId == tenantId, cancellationToken);

        if (entity == null)
            throw new ValidationException("Purchase not found.");

        if (entity.Status != "Draft")
            throw new ValidationException($"Purchase cannot be received because its current status is {entity.Status}.");

        entity.Status = "Received";
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToBO(entity);
    }
}
