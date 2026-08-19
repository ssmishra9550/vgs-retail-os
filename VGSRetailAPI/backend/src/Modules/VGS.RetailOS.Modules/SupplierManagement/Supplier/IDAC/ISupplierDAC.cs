using VGS.RetailOS.Modules.SupplierManagement.Supplier.BO;

namespace VGS.RetailOS.Modules.SupplierManagement.Supplier.IDAC;

public interface ISupplierDAC
{
    Task<SupplierBO> CreateSupplierAsync(SupplierBO supplier, CancellationToken cancellationToken);
    Task<SupplierBO> UpdateSupplierAsync(SupplierBO supplier, CancellationToken cancellationToken);
    Task UpdateOutstandingPayableAsync(Guid supplierId, string tenantId, decimal amount, CancellationToken cancellationToken);
    Task<SupplierBO?> GetSupplierByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<List<SupplierBO>> GetAllSuppliersAsync(string tenantId, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, string tenantId, Guid? excludeId, CancellationToken cancellationToken);
    Task<bool> ExistsByMobileAsync(string mobile, string tenantId, Guid? excludeId, CancellationToken cancellationToken);
    Task<bool> DeleteSupplierAsync(Guid id, string tenantId, CancellationToken cancellationToken);
}
