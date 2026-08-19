using VGS.RetailOS.Contracts.V1.Store.Requests;
using VGS.RetailOS.Contracts.V1.Store.Responses;
using VGS.RetailOS.Modules.Organization.IDAC;
using VGS.RetailOS.Modules.Store.BO;
using VGS.RetailOS.Modules.Store.IBL;
using VGS.RetailOS.Modules.Store.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.Store.BL;

public class StoreBL : IStoreBL
{
    private readonly IStoreDAC _storeDac;
    private readonly IOrganizationDAC _organizationDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public StoreBL(IStoreDAC storeDac, IOrganizationDAC organizationDac, ITenantContextAccessor tenantContextAccessor)
    {
        _storeDac = storeDac ?? throw new ArgumentNullException(nameof(storeDac));
        _organizationDac = organizationDac ?? throw new ArgumentNullException(nameof(organizationDac));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    public async Task<StoreResponse> CreateAsync(CreateStoreRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Store Name is required.");
        }

        if (request.OrganizationId == Guid.Empty)
        {
            throw new ValidationException("Organization ID is required.");
        }

        // Validate that the Organization belongs to the current tenant
        var organization = await _organizationDac.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (organization == null || organization.TenantId != tenantId)
        {
            throw new NotFoundException($"Organization with ID {request.OrganizationId} not found in the current tenant.");
        }

        // Enforce uniqueness of Name within Organization
        var nameExists = await _storeDac.ExistsByNameAsync(request.Name, request.OrganizationId, tenantId, cancellationToken: cancellationToken);
        if (nameExists)
        {
            throw new ConflictException($"A store with the name '{request.Name}' already exists in this organization.");
        }

        // Enforce uniqueness of Code within Organization (if provided)
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeExists = await _storeDac.ExistsByCodeAsync(request.Code, request.OrganizationId, tenantId, cancellationToken: cancellationToken);
            if (codeExists)
            {
                throw new ConflictException($"A store with the code '{request.Code}' already exists in this organization.");
            }
        }

        var storeBo = new StoreBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Code = request.Code,
            Address = request.Address,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createdBo = await _storeDac.CreateAsync(storeBo, cancellationToken);

        return MapToResponse(createdBo);
    }

    public async Task<StoreResponse> UpdateAsync(Guid id, UpdateStoreRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Store Name is required.");
        }

        var existingStore = await _storeDac.GetByIdAsync(id, cancellationToken);
        if (existingStore == null || existingStore.TenantId != tenantId)
        {
            throw new NotFoundException($"Store with ID {id} not found.");
        }

        // Enforce uniqueness of Name within Organization
        var nameExists = await _storeDac.ExistsByNameAsync(request.Name, existingStore.OrganizationId, tenantId, excludeId: id, cancellationToken: cancellationToken);
        if (nameExists)
        {
            throw new ConflictException($"A store with the name '{request.Name}' already exists in this organization.");
        }

        // Enforce uniqueness of Code within Organization (if provided)
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeExists = await _storeDac.ExistsByCodeAsync(request.Code, existingStore.OrganizationId, tenantId, excludeId: id, cancellationToken: cancellationToken);
            if (codeExists)
            {
                throw new ConflictException($"A store with the code '{request.Code}' already exists in this organization.");
            }
        }

        existingStore.Name = request.Name;
        existingStore.Code = request.Code;
        existingStore.Address = request.Address;
        existingStore.ContactEmail = request.ContactEmail;
        existingStore.ContactPhone = request.ContactPhone;
        existingStore.IsActive = request.IsActive;
        existingStore.UpdatedAt = DateTimeOffset.UtcNow;

        var updatedBo = await _storeDac.UpdateAsync(existingStore, cancellationToken);

        return MapToResponse(updatedBo);
    }


    public async Task<List<StoreResponse>> GetAllForTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var stores = await _storeDac.GetAllForTenantAsync(tenantId, cancellationToken);
        
        return stores.Select(MapToResponse).ToList();
    }

    public async Task<StoreResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        var store = await _storeDac.GetByIdAsync(id, cancellationToken);
        if (store == null || store.TenantId != tenantId)
        {
            throw new NotFoundException($"Store with ID {id} not found.");
        }

        return MapToResponse(store);
    }

    public async Task<List<StoreResponse>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        // Validate that the Organization belongs to the current tenant
        var organization = await _organizationDac.GetByIdAsync(organizationId, cancellationToken);
        if (organization == null || organization.TenantId != tenantId)
        {
            throw new NotFoundException($"Organization with ID {organizationId} not found in the current tenant.");
        }

        var stores = await _storeDac.GetByOrganizationIdAsync(organizationId, tenantId, cancellationToken);

        return stores.Select(MapToResponse).ToList();
    }

    private string GetTenantId()
    {
        var tenantContext = _tenantContextAccessor.TenantContext;
        if (tenantContext == null || string.IsNullOrWhiteSpace(tenantContext.CurrentTenantId))
        {
            throw new TenantNotFoundException("A valid tenant context is required to perform store operations.");
        }
        return tenantContext.CurrentTenantId;
    }

    private static StoreResponse MapToResponse(StoreBO bo)
    {
        return new StoreResponse
        {
            Id = bo.Id,
            OrganizationId = bo.OrganizationId,
            Name = bo.Name,
            Code = bo.Code,
            Address = bo.Address,
            ContactEmail = bo.ContactEmail,
            ContactPhone = bo.ContactPhone,
            IsActive = bo.IsActive,
            CreatedAt = bo.CreatedAt,
            UpdatedAt = bo.UpdatedAt
        };
    }
}
