using VGS.RetailOS.Modules.MasterData.Unit.BO;

namespace VGS.RetailOS.Modules.MasterData.Unit.IDAC;

public interface IUnitDAC
{
    Task<UnitBO?> GetUnitByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<UnitBO?> GetUnitByNameAsync(string name, string tenantId, CancellationToken cancellationToken);
    Task<List<UnitBO>> GetAllUnitsAsync(string tenantId, CancellationToken cancellationToken);
    Task<UnitBO> CreateUnitAsync(UnitBO unit, CancellationToken cancellationToken);
    Task<UnitBO> UpdateUnitAsync(UnitBO unit, CancellationToken cancellationToken);
}
