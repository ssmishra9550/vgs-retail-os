using VGS.RetailOS.Modules.MasterData.Brand.BO;

namespace VGS.RetailOS.Modules.MasterData.Brand.IDAC;

public interface IBrandDAC
{
    Task<BrandBO?> GetBrandByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<BrandBO?> GetBrandByNameAsync(string name, string tenantId, CancellationToken cancellationToken);
    Task<List<BrandBO>> GetAllBrandsAsync(string tenantId, CancellationToken cancellationToken);
    Task<BrandBO> CreateBrandAsync(BrandBO brand, CancellationToken cancellationToken);
    Task<BrandBO> UpdateBrandAsync(BrandBO brand, CancellationToken cancellationToken);
}
