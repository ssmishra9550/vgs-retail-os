using VGS.RetailOS.Contracts.V1.SupplierManagement.Requests;
using VGS.RetailOS.Contracts.V1.SupplierManagement.Responses;

namespace VGS.RetailOS.Modules.SupplierManagement.Supplier.IBL;

public interface ISupplierBL
{
    Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken);
    Task<SupplierResponse> UpdateSupplierAsync(UpdateSupplierRequest request, CancellationToken cancellationToken);
    Task<SupplierResponse?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<SupplierResponse>> GetAllSuppliersAsync(CancellationToken cancellationToken);
    Task UpdateOutstandingPayableAsync(Guid supplierId, decimal amount, CancellationToken cancellationToken);
    Task DeleteSupplierAsync(Guid id, CancellationToken cancellationToken);
}
