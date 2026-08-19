using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;
using VGS.RetailOS.Modules.MasterData.Tax.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.MasterData;

[ApiController]
[Route("api/v1/taxes")]
[Authorize]
public class TaxController : ControllerBase
{
    private readonly ITaxBL _taxBl;

    public TaxController(ITaxBL taxBl)
    {
        _taxBl = taxBl;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TaxResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTaxes(CancellationToken cancellationToken)
    {
        var result = await _taxBl.GetAllTaxesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taxBl.GetTaxByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaxResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTax([FromBody] CreateTaxRequest request, CancellationToken cancellationToken)
    {
        var result = await _taxBl.CreateTaxAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTaxById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTax(Guid id, [FromBody] UpdateTaxRequest request, CancellationToken cancellationToken)
    {
        var result = await _taxBl.UpdateTaxAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
