using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.SupplierManagement.Requests;
using VGS.RetailOS.Contracts.V1.SupplierManagement.Responses;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.SupplierManagement;

[ApiController]
[Route("api/v1/suppliers")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierBL _supplierBl;

    public SupplierController(ISupplierBL supplierBl)
    {
        _supplierBl = supplierBl;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await _supplierBl.CreateSupplierAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSupplier), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Id in route must match Id in request body.");

        var result = await _supplierBl.UpdateSupplierAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplier(Guid id, CancellationToken cancellationToken)
    {
        var result = await _supplierBl.GetSupplierByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSuppliers(CancellationToken cancellationToken)
    {
        var result = await _supplierBl.GetAllSuppliersAsync(cancellationToken);
        return Ok(result);
    }
}
