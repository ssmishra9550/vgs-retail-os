using VGS.RetailOS.Infrastructure.Store.DAC.Entities;
using VGS.RetailOS.Modules.Store.BO;

namespace VGS.RetailOS.Infrastructure.Store.DAC.Mapping;

public static class StoreMapper
{
    public static StoreBO ToStoreBO(this StoreEntity entity)
    {
        return new StoreBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            OrganizationId = entity.OrganizationId,
            Name = entity.Name,
            Code = entity.Code,
            Address = entity.Address,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static StoreEntity ToEntity(this StoreBO bo)
    {
        return new StoreEntity
        {
            Id = bo.Id,
            TenantId = bo.TenantId,
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
