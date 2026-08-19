using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;

namespace VGS.RetailOS.Modules.MasterData.Unit.IBL;

public interface IUnitBL
{
    Task<UnitResponse> GetUnitByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<UnitResponse>> GetAllUnitsAsync(CancellationToken cancellationToken);
    Task<UnitResponse> CreateUnitAsync(CreateUnitRequest request, CancellationToken cancellationToken);
    Task<UnitResponse> UpdateUnitAsync(Guid id, UpdateUnitRequest request, CancellationToken cancellationToken);
}
