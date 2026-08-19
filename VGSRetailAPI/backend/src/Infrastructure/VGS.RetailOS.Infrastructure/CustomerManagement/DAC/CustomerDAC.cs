using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.CustomerManagement.DAC.Entities;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Modules.CustomerManagement.Customer.BO;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;

namespace VGS.RetailOS.Infrastructure.CustomerManagement.DAC;

public class CustomerDAC : ICustomerDAC
{
    private readonly AppDbContext _dbContext;

    public CustomerDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerBO?> GetCustomerByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);

        return entity == null ? null : MapToBO(entity);
    }

    public async Task<CustomerBO?> GetCustomerByMobileAsync(string mobile, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Mobile == mobile && c.TenantId == tenantId, cancellationToken);

        return entity == null ? null : MapToBO(entity);
    }

    public async Task<List<CustomerBO>> GetAllCustomersAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Customers
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToBO).ToList();
    }

    public async Task<CustomerBO> CreateCustomerAsync(CustomerBO customer, CancellationToken cancellationToken)
    {
        var entity = new CustomerEntity
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Mobile = customer.Mobile,
            Email = customer.Email,
            Address = customer.Address,
            CreditBalance = customer.CreditBalance,
            IsActive = customer.IsActive
        };

        _dbContext.Customers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToBO(entity);
    }

    public async Task UpdateCreditBalanceAsync(Guid customerId, string tenantId, decimal amount, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.TenantId == tenantId, cancellationToken);
        if (entity == null)
            throw new ValidationException("Customer not found.");

        entity.CreditBalance += amount;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerBO> UpdateCustomerAsync(CustomerBO customer, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == customer.Id && c.TenantId == customer.TenantId, cancellationToken);

        if (entity != null)
        {
            entity.FirstName = customer.FirstName;
            entity.LastName = customer.LastName;
            entity.Mobile = customer.Mobile;
            entity.Email = customer.Email;
            entity.Address = customer.Address;
            // Deliberately NOT updating CreditBalance as per design constraints
            entity.IsActive = customer.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapToBO(entity);
        }

        return customer;
    }

    private static CustomerBO MapToBO(CustomerEntity entity)
    {
        return new CustomerBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Mobile = entity.Mobile,
            Email = entity.Email,
            Address = entity.Address,
            CreditBalance = entity.CreditBalance,
            IsActive = entity.IsActive
        };
    }
}
