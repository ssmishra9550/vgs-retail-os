using VGS.RetailOS.Modules.CustomerManagement.Customer.BO;

namespace VGS.RetailOS.Modules.CustomerManagement.Customer.IDAC;

public interface ICustomerDAC
{
    Task<CustomerBO?> GetCustomerByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<CustomerBO?> GetCustomerByMobileAsync(string mobile, string tenantId, CancellationToken cancellationToken);
    Task<List<CustomerBO>> GetAllCustomersAsync(string tenantId, CancellationToken cancellationToken);
    Task<CustomerBO> CreateCustomerAsync(CustomerBO customer, CancellationToken cancellationToken);
    Task<CustomerBO> UpdateCustomerAsync(CustomerBO customer, CancellationToken cancellationToken);
    Task UpdateCreditBalanceAsync(Guid customerId, string tenantId, decimal amount, CancellationToken cancellationToken);
}
