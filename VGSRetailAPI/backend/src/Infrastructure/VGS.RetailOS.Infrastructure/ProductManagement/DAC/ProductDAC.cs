using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.ProductManagement.DAC.Entities;
using VGS.RetailOS.Modules.ProductManagement.Product.BO;
using VGS.RetailOS.Modules.ProductManagement.Product.IDAC;

namespace VGS.RetailOS.Infrastructure.ProductManagement.DAC;

public class ProductDAC : IProductDAC
{
    private readonly AppDbContext _dbContext;

    public ProductDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductBO?> GetProductByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Unit)
            .Include(p => p.Tax)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<ProductBO?> GetProductBySkuAsync(string sku, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Unit)
            .Include(p => p.Tax)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Sku == sku && p.TenantId == tenantId, cancellationToken);
            
        return MapToBO(entity);
    }

    public async Task<List<ProductBO>> GetAllProductsAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Unit)
            .Include(p => p.Tax)
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
            
        return entities.Select(MapToBO).Where(x => x != null).Cast<ProductBO>().ToList();
    }

    public async Task<ProductBO> CreateProductAsync(ProductBO product, CancellationToken cancellationToken)
    {
        var entity = new ProductEntity
        {
            Id = product.Id,
            TenantId = product.TenantId,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            PurchasePrice = product.PurchasePrice,
            SellingPrice = product.SellingPrice,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            UnitId = product.UnitId,
            TaxId = product.TaxId,
            IsActive = product.IsActive
        };

        _dbContext.Products.Add(entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.GetType().Name == "PostgresException")
        {
            throw new VGS.RetailOS.Shared.Errors.Exceptions.ValidationException("One or more referenced master data IDs (Category, Brand, Unit, Tax) are invalid.");
        }

        // Fetch back to get the navigation properties filled
        return (await GetProductByIdAsync(entity.Id, entity.TenantId, cancellationToken))!;
    }

    public async Task<ProductBO> UpdateProductAsync(ProductBO product, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id && p.TenantId == product.TenantId, cancellationToken);

        if (entity != null)
        {
            entity.Name = product.Name;
            entity.Sku = product.Sku;
            entity.Description = product.Description;
            entity.PurchasePrice = product.PurchasePrice;
            entity.SellingPrice = product.SellingPrice;
            entity.CategoryId = product.CategoryId;
            entity.BrandId = product.BrandId;
            entity.UnitId = product.UnitId;
            entity.TaxId = product.TaxId;
            entity.IsActive = product.IsActive;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("23503") == true || ex.InnerException?.GetType().Name == "PostgresException")
            {
                throw new VGS.RetailOS.Shared.Errors.Exceptions.ValidationException("One or more referenced master data IDs (Category, Brand, Unit, Tax) are invalid.");
            }
        }

        return (await GetProductByIdAsync(product.Id, product.TenantId, cancellationToken))!;
    }

    public async Task<bool> DeleteProductAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
            
        if (entity == null) return false;
        
        _dbContext.Products.Remove(entity); // EF will intercept and perform soft delete
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private ProductBO? MapToBO(ProductEntity? entity)
    {
        if (entity == null) return null;
        
        return new ProductBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Sku = entity.Sku,
            Description = entity.Description,
            PurchasePrice = entity.PurchasePrice,
            SellingPrice = entity.SellingPrice,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name,
            BrandId = entity.BrandId,
            BrandName = entity.Brand?.Name,
            UnitId = entity.UnitId,
            UnitName = entity.Unit?.Name,
            TaxId = entity.TaxId,
            TaxName = entity.Tax?.Name,
            IsActive = entity.IsActive
        };
    }
}
