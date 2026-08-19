using VGS.RetailOS.Contracts.V1.SupplierManagement.Requests;
using VGS.RetailOS.Contracts.V1.SupplierManagement.Responses;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.BO;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IBL;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.SupplierManagement.Supplier.BL;

public class SupplierBL : ISupplierBL
{
    private readonly ISupplierDAC _supplierDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public SupplierBL(ISupplierDAC supplierDac, ITenantContextAccessor tenantContextAccessor)
    {
        _supplierDac = supplierDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        await ValidateNameAndMobileUniqueness(request.Name, request.Mobile, tenantId, null, cancellationToken);

        var supplierBo = new SupplierBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Mobile = request.Mobile,
            Email = request.Email,
            GstNumber = request.GstNumber,
            Address = request.Address,
            OutstandingPayable = 0, // Starts at zero
            IsActive = request.IsActive
        };

        var createdBo = await _supplierDac.CreateSupplierAsync(supplierBo, cancellationToken);
        return MapToResponse(createdBo);
    }

    public async Task<SupplierResponse> UpdateSupplierAsync(UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        await ValidateNameAndMobileUniqueness(request.Name, request.Mobile, tenantId, request.Id, cancellationToken);

        var existingSupplier = await _supplierDac.GetSupplierByIdAsync(request.Id, tenantId, cancellationToken);
        if (existingSupplier == null)
            throw new NotFoundException($"Supplier with ID {request.Id} not found.");

        existingSupplier.Name = request.Name;
        existingSupplier.ContactPerson = request.ContactPerson;
        existingSupplier.Mobile = request.Mobile;
        existingSupplier.Email = request.Email;
        existingSupplier.GstNumber = request.GstNumber;
        existingSupplier.Address = request.Address;
        existingSupplier.IsActive = request.IsActive;
        // Notice we DO NOT update OutstandingPayable here. It is excluded from the request.

        var bo = await _supplierDac.UpdateSupplierAsync(existingSupplier, cancellationToken);
        return MapToResponse(bo);
    }

    public async Task UpdateOutstandingPayableAsync(Guid supplierId, decimal amount, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        await _supplierDac.UpdateOutstandingPayableAsync(supplierId, tenantId, amount, cancellationToken);
    }

    public async Task<SupplierResponse?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var supplier = await _supplierDac.GetSupplierByIdAsync(id, tenantId, cancellationToken);
        return supplier == null ? null : MapToResponse(supplier);
    }

    public async Task<List<SupplierResponse>> GetAllSuppliersAsync(CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var suppliers = await _supplierDac.GetAllSuppliersAsync(tenantId, cancellationToken);
        return suppliers.Select(MapToResponse).ToList();
    }

    private async Task ValidateNameAndMobileUniqueness(string name, string mobile, string tenantId, Guid? excludeId, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (await _supplierDac.ExistsByNameAsync(name, tenantId, excludeId, cancellationToken))
            errors.Add($"A supplier with the name '{name}' already exists.");

        if (await _supplierDac.ExistsByMobileAsync(mobile, tenantId, excludeId, cancellationToken))
            errors.Add($"A supplier with the mobile '{mobile}' already exists.");

        if (errors.Any())
            throw new ValidationException(string.Join(" ", errors));
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    
    public async Task DeleteSupplierAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var success = await _supplierDac.DeleteSupplierAsync(id, tenantId, cancellationToken);
        if (!success)
            throw new NotFoundException($"'Supplier' with ID {id} not found.");
    }
private static SupplierResponse MapToResponse(SupplierBO bo)
    {
        return new SupplierResponse
        {
            Id = bo.Id,
            Name = bo.Name,
            ContactPerson = bo.ContactPerson,
            Mobile = bo.Mobile,
            Email = bo.Email,
            GstNumber = bo.GstNumber,
            Address = bo.Address,
            OutstandingPayable = bo.OutstandingPayable,
            IsActive = bo.IsActive
        };
    }
}
