using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.CustomerManagement.Requests;
using VGS.RetailOS.Contracts.V1.CustomerManagement.Responses;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.CustomerManagement;

[ApiController]
[Route("api/v1/customers")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerBL _customerBl;

    public CustomerController(ICustomerBL customerBl)
    {
        _customerBl = customerBl;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCustomers(CancellationToken cancellationToken)
    {
        var result = await _customerBl.GetAllCustomersAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customerBl.GetCustomerByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerBl.CreateCustomerAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCustomerById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerBl.UpdateCustomerAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
