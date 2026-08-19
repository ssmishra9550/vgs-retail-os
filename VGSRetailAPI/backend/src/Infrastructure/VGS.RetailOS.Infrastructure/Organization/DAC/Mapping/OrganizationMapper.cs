using VGS.RetailOS.Infrastructure.Organization.DAC.Entities;
using VGS.RetailOS.Modules.Organization.BO;

namespace VGS.RetailOS.Infrastructure.Organization.DAC.Mapping;

public static class OrganizationMapper
{
    public static OrganizationBO ToBo(OrganizationEntity entity)
    {
        return new OrganizationBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Code = entity.Code,
            TaxId = entity.TaxId,
            Address = entity.Address,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static OrganizationEntity ToEntity(OrganizationBO bo)
    {
        return new OrganizationEntity
        {
            Id = bo.Id,
            TenantId = bo.TenantId,
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
