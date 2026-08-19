using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;

namespace VGS.RetailOS.Modules.MasterData.Tax.IBL;

public interface ITaxBL
{
    Task<TaxResponse> GetTaxByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TaxResponse>> GetAllTaxesAsync(CancellationToken cancellationToken);
    Task<TaxResponse> CreateTaxAsync(CreateTaxRequest request, CancellationToken cancellationToken);
    Task<TaxResponse> UpdateTaxAsync(Guid id, UpdateTaxRequest request, CancellationToken cancellationToken);
}
