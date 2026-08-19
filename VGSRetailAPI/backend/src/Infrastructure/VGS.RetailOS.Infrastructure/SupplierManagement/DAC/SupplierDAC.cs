using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.SupplierManagement.DAC.Entities;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.BO;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;

namespace VGS.RetailOS.Infrastructure.SupplierManagement.DAC;

public class SupplierDAC : ISupplierDAC
{
    private readonly AppDbContext _dbContext;

    public SupplierDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SupplierBO> CreateSupplierAsync(SupplierBO supplier, CancellationToken cancellationToken)
    {
        var entity = new SupplierEntity
        {
            Id = supplier.Id,
            TenantId = supplier.TenantId,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Mobile = supplier.Mobile,
            Email = supplier.Email,
            GstNumber = supplier.GstNumber,
            Address = supplier.Address,
            OutstandingPayable = supplier.OutstandingPayable,
            IsActive = supplier.IsActive
        };

        _dbContext.Suppliers.Add(entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505") // Unique constraint violation
            {
                throw new ValidationException("Supplier with the given Name or Mobile already exists.");
            }
            throw;
        }

        return MapToBO(entity);
    }

    public async Task<SupplierBO> UpdateSupplierAsync(SupplierBO supplier, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == supplier.Id && s.TenantId == supplier.TenantId, cancellationToken);
        if (entity == null)
            throw new ValidationException("Supplier not found.");

        entity.Name = supplier.Name;
        entity.ContactPerson = supplier.ContactPerson;
        entity.Mobile = supplier.Mobile;
        entity.Email = supplier.Email;
        entity.GstNumber = supplier.GstNumber;
        entity.Address = supplier.Address;
        entity.IsActive = supplier.IsActive;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505")
            {
                throw new ValidationException("Supplier with the given Name or Mobile already exists.");
            }
            throw;
        }

        return MapToBO(entity);
    }

    public async Task UpdateOutstandingPayableAsync(Guid supplierId, string tenantId, decimal amount, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId && s.TenantId == tenantId, cancellationToken);
        if (entity == null)
            throw new ValidationException("Supplier not found.");

        entity.OutstandingPayable += amount;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SupplierBO?> GetSupplierByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);
        return entity == null ? null : MapToBO(entity);
    }

    public async Task<List<SupplierBO>> GetAllSuppliersAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Suppliers.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);
        return entities.Select(MapToBO).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string name, string tenantId, Guid? excludeId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Suppliers.Where(s => s.Name == name && s.TenantId == tenantId);
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByMobileAsync(string mobile, string tenantId, Guid? excludeId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Suppliers.Where(s => s.Mobile == mobile && s.TenantId == tenantId);
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    private static SupplierBO MapToBO(SupplierEntity entity)
    {
        return new SupplierBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            ContactPerson = entity.ContactPerson,
            Mobile = entity.Mobile,
            Email = entity.Email,
            GstNumber = entity.GstNumber,
            Address = entity.Address,
            OutstandingPayable = entity.OutstandingPayable,
            IsActive = entity.IsActive
        };
    }
}
