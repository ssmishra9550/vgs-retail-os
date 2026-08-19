using VGS.RetailOS.Modules.ProductManagement.Product.BO;

namespace VGS.RetailOS.Modules.ProductManagement.Product.IDAC;

public interface IProductDAC
{
    Task<ProductBO?> GetProductByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<ProductBO?> GetProductBySkuAsync(string sku, string tenantId, CancellationToken cancellationToken);
    Task<List<ProductBO>> GetAllProductsAsync(string tenantId, CancellationToken cancellationToken);
    Task<ProductBO> CreateProductAsync(ProductBO product, CancellationToken cancellationToken);
    Task<ProductBO> UpdateProductAsync(ProductBO product, CancellationToken cancellationToken);
    Task<bool> DeleteProductAsync(Guid id, string tenantId, CancellationToken cancellationToken);
}
