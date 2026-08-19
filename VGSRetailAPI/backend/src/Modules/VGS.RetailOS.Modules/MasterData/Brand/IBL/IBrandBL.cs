using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;

namespace VGS.RetailOS.Modules.MasterData.Brand.IBL;

public interface IBrandBL
{
    Task<BrandResponse> GetBrandByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<BrandResponse>> GetAllBrandsAsync(CancellationToken cancellationToken);
    Task<BrandResponse> CreateBrandAsync(CreateBrandRequest request, CancellationToken cancellationToken);
    Task<BrandResponse> UpdateBrandAsync(Guid id, UpdateBrandRequest request, CancellationToken cancellationToken);
}
