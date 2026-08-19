using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Contracts.V1.InventoryManagement.Responses;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.InventoryManagement;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryBL _inventoryBl;

    public InventoryController(IInventoryBL inventoryBl)
    {
        _inventoryBl = inventoryBl;
    }

    [HttpGet("balance/{storeId}/{productId}")]
    public async Task<ActionResult<StockBalanceResponse>> GetStockBalance(Guid storeId, Guid productId, CancellationToken cancellationToken)
    {
        var result = await _inventoryBl.GetStockBalanceAsync(storeId, productId, cancellationToken);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("balance/{storeId}")]
    public async Task<ActionResult<List<StockBalanceResponse>>> GetAllStockBalances(Guid storeId, CancellationToken cancellationToken)
    {
        var result = await _inventoryBl.GetAllStockBalancesAsync(storeId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history/{storeId}/{productId}")]
    public async Task<ActionResult<List<InventoryLedgerResponse>>> GetStockHistory(Guid storeId, Guid productId, CancellationToken cancellationToken)
    {
        var result = await _inventoryBl.GetStockHistoryAsync(storeId, productId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Intended for direct manual stock adjustments (corrections, damage write-offs). 
    /// For Purchase/Sale, the respective module should call IInventoryBL internally.
    /// </summary>
    [HttpPost("adjust")]
    public async Task<ActionResult<InventoryLedgerResponse>> RecordStockTransaction([FromBody] RecordStockTransactionRequest request, CancellationToken cancellationToken)
    {
        // Enforce that only adjustments come through this public API
        if (request.TransactionType != "Adjustment" && request.TransactionType != "InitialStock")
        {
            return BadRequest(new { Message = "Only manual Adjustments or InitialStock can be made via this endpoint." });
        }

        var result = await _inventoryBl.RecordTransactionAsync(request, cancellationToken);
        return Ok(result);
    }
}
