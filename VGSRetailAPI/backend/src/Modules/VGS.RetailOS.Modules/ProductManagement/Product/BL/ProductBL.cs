
using VGS.RetailOS.Contracts.V1.ProductManagement.Requests;
using VGS.RetailOS.Contracts.V1.ProductManagement.Responses;
using VGS.RetailOS.Modules.ProductManagement.Product.BO;
using VGS.RetailOS.Modules.ProductManagement.Product.IBL;
using VGS.RetailOS.Modules.ProductManagement.Product.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.ProductManagement.Product.BL;

public class ProductBL : IProductBL
{
    private readonly IProductDAC _productDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public ProductBL(IProductDAC productDac, ITenantContextAccessor tenantContextAccessor)
    {
        _productDac = productDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<ProductResponse> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var product = await _productDac.GetProductByIdAsync(id, tenantId, cancellationToken);
        
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found.");

        return MapToResponse(product);
    }

    public async Task<List<ProductResponse>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var products = await _productDac.GetAllProductsAsync(tenantId, cancellationToken);
        
        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var existing = await _productDac.GetProductBySkuAsync(request.Sku, tenantId, cancellationToken);
            if (existing != null)
                throw new ValidationException($"Product with SKU '{request.Sku}' already exists.");
        }

        var productBo = new ProductBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Sku = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku,
            Description = request.Description,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            UnitId = request.UnitId,
            TaxId = request.TaxId,
            IsActive = true
        };

        var created = await _productDac.CreateProductAsync(productBo, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var product = await _productDac.GetProductByIdAsync(id, tenantId, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Sku) && !string.Equals(product.Sku, request.Sku, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _productDac.GetProductBySkuAsync(request.Sku, tenantId, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new ValidationException($"Product with SKU '{request.Sku}' already exists.");
        }

        product.Name = request.Name;
        product.Sku = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku;
        product.Description = request.Description;
        product.PurchasePrice = request.PurchasePrice;
        product.SellingPrice = request.SellingPrice;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.UnitId = request.UnitId;
        product.TaxId = request.TaxId;
        product.IsActive = request.IsActive;

        var updated = await _productDac.UpdateProductAsync(product, cancellationToken);
        return MapToResponse(updated);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    private ProductResponse MapToResponse(ProductBO bo)
    {
        return new ProductResponse
        {
            Id = bo.Id,
            Name = bo.Name,
            Sku = bo.Sku,
            Description = bo.Description,
            PurchasePrice = bo.PurchasePrice,
            SellingPrice = bo.SellingPrice,
            CategoryId = bo.CategoryId,
            CategoryName = bo.CategoryName,
            BrandId = bo.BrandId,
            BrandName = bo.BrandName,
            UnitId = bo.UnitId,
            UnitName = bo.UnitName,
            TaxId = bo.TaxId,
            TaxName = bo.TaxName,
            IsActive = bo.IsActive
        };
    }
}
