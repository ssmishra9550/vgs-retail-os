using VGS.RetailOS.Contracts.V1.Organization.Requests;
using VGS.RetailOS.Contracts.V1.Organization.Responses;
using VGS.RetailOS.Modules.Organization.BO;
using VGS.RetailOS.Modules.Organization.IBL;
using VGS.RetailOS.Modules.Organization.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.Organization.BL;

public class OrganizationBL : IOrganizationBL
{
    private readonly IOrganizationDAC _organizationDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public OrganizationBL(IOrganizationDAC organizationDac, ITenantContextAccessor tenantContextAccessor)
    {
        _organizationDac = organizationDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    private string CurrentTenantId
    {
        get
        {
            var context = _tenantContextAccessor.TenantContext;
            if (context == null || !context.IsTenantResolved)
            {
                throw new TenantNotFoundException();
            }
            return context.CurrentTenantId;
        }
    }

    public async Task<OrganizationResponse> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId;

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Organization Name is required.");
        }

        // Check for duplicate name within the same tenant
        var exists = await _organizationDac.ExistsByNameAsync(request.Name, tenantId, null, cancellationToken);
        if (exists)
        {
            throw new ConflictException($"An organization with the name '{request.Name}' already exists.");
        }

        var organizationBo = new OrganizationBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Code = request.Code,
            TaxId = request.TaxId,
            Address = request.Address,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createdOrganization = await _organizationDac.CreateAsync(organizationBo, cancellationToken);
        
        return MapToResponse(createdOrganization);
    }

    public async Task<OrganizationResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Tenant context is injected and enforced via EF Core Global Query Filters in the DAC.
        var organization = await _organizationDac.GetByIdAsync(id, cancellationToken);
        if (organization == null)
        {
            throw new NotFoundException($"Organization with ID {id} was not found.");
        }

        return MapToResponse(organization);
    }

    public async Task<OrganizationResponse> UpdateAsync(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Organization Name is required.");
        }

        var existingOrganization = await _organizationDac.GetByIdAsync(id, cancellationToken);
        if (existingOrganization == null)
        {
            throw new NotFoundException($"Organization with ID {id} was not found.");
        }

        // Ensure new name doesn't conflict with another organization in the same tenant
        var exists = await _organizationDac.ExistsByNameAsync(request.Name, tenantId, id, cancellationToken);
        if (exists)
        {
            throw new ConflictException($"An organization with the name '{request.Name}' already exists.");
        }

        var updatedBo = new OrganizationBO
        {
            Id = existingOrganization.Id,
            TenantId = existingOrganization.TenantId,
            Name = request.Name,
            Code = request.Code,
            TaxId = request.TaxId,
            Address = request.Address,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CreatedAt = existingOrganization.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var savedOrganization = await _organizationDac.UpdateAsync(updatedBo, cancellationToken);
        
        return MapToResponse(savedOrganization);
    }

    private static OrganizationResponse MapToResponse(OrganizationBO bo)
    {
        return new OrganizationResponse
        {
            Id = bo.Id,
            Name = bo.Name,
            Code = bo.Code,
            TaxId = bo.TaxId,
            Address = bo.Address,
            ContactEmail = bo.ContactEmail,
            ContactPhone = bo.ContactPhone,
            CreatedAt = bo.CreatedAt,
            UpdatedAt = bo.UpdatedAt
        };
    }
}
