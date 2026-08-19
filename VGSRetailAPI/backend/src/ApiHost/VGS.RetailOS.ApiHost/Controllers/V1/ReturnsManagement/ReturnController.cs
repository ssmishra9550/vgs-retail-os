using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.ReturnsManagement.Requests;
using VGS.RetailOS.Modules.ReturnsManagement.Return.IBL;
namespace VGS.RetailOS.ApiHost.Controllers.V1.ReturnsManagement;

[ApiController]
[Route("api/v1/return")]
[Authorize]
public class ReturnController : ControllerBase
{
    private readonly IReturnBL _bl;
    public ReturnController(IReturnBL bl) { _bl = bl; }

    [HttpPost]
    public async Task<IActionResult> CreateReturn([FromBody] CreateReturnRequest request, CancellationToken cancellationToken)
    {
        var res = await _bl.ProcessReturnAsync(request, cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _bl.GetAllReturnsAsync(cancellationToken));
    }
}
