using VGS.RetailOS.Modules.MasterData.Tax.BO;

namespace VGS.RetailOS.Modules.MasterData.Tax.IDAC;

public interface ITaxDAC
{
    Task<TaxBO?> GetTaxByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<TaxBO?> GetTaxByNameAsync(string name, string tenantId, CancellationToken cancellationToken);
    Task<List<TaxBO>> GetAllTaxesAsync(string tenantId, CancellationToken cancellationToken);
    Task<TaxBO> CreateTaxAsync(TaxBO tax, CancellationToken cancellationToken);
    Task<TaxBO> UpdateTaxAsync(TaxBO tax, CancellationToken cancellationToken);
}
