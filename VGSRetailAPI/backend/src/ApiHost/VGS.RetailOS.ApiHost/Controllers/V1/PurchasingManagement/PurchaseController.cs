using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.PurchasingManagement.Requests;
using VGS.RetailOS.Contracts.V1.PurchasingManagement.Responses;
using VGS.RetailOS.Modules.PurchasingManagement.Purchase.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.PurchasingManagement;

[ApiController]
[Route("api/v1/purchases")]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseBL _purchaseBl;

    public PurchaseController(IPurchaseBL purchaseBl)
    {
        _purchaseBl = purchaseBl;
    }

    [HttpPost("drafts")]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDraftPurchase([FromBody] CreatePurchaseRequest request, CancellationToken cancellationToken)
    {
        var response = await _purchaseBl.CreateDraftPurchaseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPurchaseById), new { id = response.Id }, response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _purchaseBl.GetPurchaseByIdAsync(id, cancellationToken);
        if (response == null) return NotFound();
        
        return Ok(response);
    }

    [HttpGet("store/{storeId}")]
    [ProducesResponseType(typeof(IEnumerable<PurchaseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPurchases(Guid storeId, CancellationToken cancellationToken)
    {
        var response = await _purchaseBl.GetAllPurchasesAsync(storeId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id}/receive")]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReceivePurchase(Guid id, CancellationToken cancellationToken)
    {
        var response = await _purchaseBl.ReceivePurchaseAsync(id, cancellationToken);
        return Ok(response);
    }
}
