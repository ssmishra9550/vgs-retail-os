using VGS.RetailOS.Contracts.V1.CustomerManagement.Requests;
using VGS.RetailOS.Contracts.V1.CustomerManagement.Responses;

namespace VGS.RetailOS.Modules.CustomerManagement.Customer.IBL;

public interface ICustomerBL
{
    Task<CustomerResponse> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<CustomerResponse>> GetAllCustomersAsync(CancellationToken cancellationToken);
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
    Task<CustomerResponse> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken);
    Task UpdateCreditBalanceAsync(Guid customerId, decimal amount, CancellationToken cancellationToken);
    Task DeleteCustomerAsync(Guid id, CancellationToken cancellationToken);
}
