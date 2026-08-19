using VGS.RetailOS.Contracts.V1.CustomerManagement.Requests;
using VGS.RetailOS.Contracts.V1.CustomerManagement.Responses;
using VGS.RetailOS.Modules.CustomerManagement.Customer.BO;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IBL;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.CustomerManagement.Customer.BL;

public class CustomerBL : ICustomerBL
{
    private readonly ICustomerDAC _customerDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public CustomerBL(ICustomerDAC customerDac, ITenantContextAccessor tenantContextAccessor)
    {
        _customerDac = customerDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<CustomerResponse> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var customer = await _customerDac.GetCustomerByIdAsync(id, tenantId, cancellationToken);
        
        if (customer == null)
            throw new NotFoundException($"Customer with ID {id} not found.");

        return MapToResponse(customer);
    }

    public async Task<List<CustomerResponse>> GetAllCustomersAsync(CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var customers = await _customerDac.GetAllCustomersAsync(tenantId, cancellationToken);
        
        return customers.Select(MapToResponse).ToList();
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var existing = await _customerDac.GetCustomerByMobileAsync(request.Mobile, tenantId, cancellationToken);
        if (existing != null)
            throw new ValidationException($"Customer with mobile number '{request.Mobile}' already exists.");

        var customerBo = new CustomerBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Mobile = request.Mobile,
            Email = request.Email,
            Address = request.Address,
            CreditBalance = 0, // Set to 0 on creation
            IsActive = true
        };

        var created = await _customerDac.CreateCustomerAsync(customerBo, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<CustomerResponse> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var customer = await _customerDac.GetCustomerByIdAsync(id, tenantId, cancellationToken);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {id} not found.");

        if (!string.Equals(customer.Mobile, request.Mobile, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _customerDac.GetCustomerByMobileAsync(request.Mobile, tenantId, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new ValidationException($"Customer with mobile number '{request.Mobile}' already exists.");
        }

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Mobile = request.Mobile;
        customer.Email = request.Email;
        customer.Address = request.Address;
        customer.IsActive = request.IsActive;

        var updated = await _customerDac.UpdateCustomerAsync(customer, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task UpdateCreditBalanceAsync(Guid customerId, decimal amount, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        await _customerDac.UpdateCreditBalanceAsync(customerId, tenantId, amount, cancellationToken);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    
    public async Task DeleteCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var success = await _customerDac.DeleteCustomerAsync(id, tenantId, cancellationToken);
        if (!success)
            throw new NotFoundException($"'Customer' with ID {id} not found.");
    }
private static CustomerResponse MapToResponse(CustomerBO bo)
    {
        return new CustomerResponse
        {
            Id = bo.Id,
            FirstName = bo.FirstName,
            LastName = bo.LastName,
            Mobile = bo.Mobile,
            Email = bo.Email,
            Address = bo.Address,
            CreditBalance = bo.CreditBalance,
            IsActive = bo.IsActive
        };
    }
}
