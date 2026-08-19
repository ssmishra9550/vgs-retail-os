using VGS.RetailOS.Contracts.V1.ProductManagement.Requests;
using VGS.RetailOS.Contracts.V1.ProductManagement.Responses;

namespace VGS.RetailOS.Modules.ProductManagement.Product.IBL;

public interface IProductBL
{
    Task<ProductResponse> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ProductResponse>> GetAllProductsAsync(CancellationToken cancellationToken);
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken);
}
