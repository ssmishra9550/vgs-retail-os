using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;
using VGS.RetailOS.Modules.MasterData.Unit.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.MasterData;

[ApiController]
[Route("api/v1/units")]
[Authorize]
public class UnitController : ControllerBase
{
    private readonly IUnitBL _unitBl;

    public UnitController(IUnitBL unitBl)
    {
        _unitBl = unitBl;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UnitResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUnits(CancellationToken cancellationToken)
    {
        var result = await _unitBl.GetAllUnitsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUnitById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _unitBl.GetUnitByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
    {
        var result = await _unitBl.CreateUnitAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUnitById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateUnitRequest request, CancellationToken cancellationToken)
    {
        var result = await _unitBl.UpdateUnitAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
