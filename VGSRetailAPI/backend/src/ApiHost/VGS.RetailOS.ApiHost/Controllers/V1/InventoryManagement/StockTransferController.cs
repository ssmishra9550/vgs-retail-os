using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IBL;
namespace VGS.RetailOS.ApiHost.Controllers.V1.InventoryManagement;

[ApiController]
[Route("api/v1/stock-transfer")]
[Authorize]
public class StockTransferController : ControllerBase
{
    private readonly IStockTransferBL _bl;
    public StockTransferController(IStockTransferBL bl) { _bl = bl; }

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiateStockTransferRequest request, CancellationToken cancellationToken)
    {
        var res = await _bl.InitiateTransferAsync(request, cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _bl.GetAllTransfersAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _bl.GetTransferAsync(id, cancellationToken));
    }
}
