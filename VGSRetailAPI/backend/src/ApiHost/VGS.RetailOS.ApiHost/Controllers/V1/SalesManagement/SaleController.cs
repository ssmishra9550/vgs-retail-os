using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.SalesManagement.Requests;
using VGS.RetailOS.Contracts.V1.SalesManagement.Responses;
using VGS.RetailOS.Modules.SalesManagement.Sale.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.SalesManagement;

[ApiController]
[Route("api/v1/sales")]
[Authorize]
public class SaleController : ControllerBase
{
    private readonly ISaleBL _saleBl;

    public SaleController(ISaleBL saleBl)
    {
        _saleBl = saleBl;
    }

    [HttpPost("drafts")]
    public async Task<ActionResult<SaleResponse>> CreateDraftSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var response = await _saleBl.CreateDraftSaleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSaleById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleResponse>> GetSaleById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _saleBl.GetSaleByIdAsync(id, cancellationToken);
        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<SaleResponse>> CompleteSale(Guid id, CancellationToken cancellationToken)
    {
        var response = await _saleBl.CompleteSaleAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("store/{storeId:guid}/drafts")]
    public async Task<ActionResult<IEnumerable<SaleResponse>>> GetDraftSales(Guid storeId, CancellationToken cancellationToken)
    {
        var response = await _saleBl.GetDraftSalesAsync(storeId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("store/{storeId:guid}/history")]
    public async Task<ActionResult<IEnumerable<SaleResponse>>> GetSalesHistory(Guid storeId, CancellationToken cancellationToken)
    {
        var response = await _saleBl.GetSalesHistoryAsync(storeId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<SaleResponse>> CancelSale(Guid id, CancellationToken cancellationToken)
    {
        var response = await _saleBl.CancelSaleAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<SaleResponse>> ProcessReturn(Guid id, [FromBody] ProcessReturnRequest request, CancellationToken cancellationToken)
    {
        var response = await _saleBl.ProcessReturnAsync(id, request, cancellationToken);
        return Ok(response);
    }
}
