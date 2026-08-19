using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;
using VGS.RetailOS.Modules.MasterData.Brand.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.MasterData;

[ApiController]
[Route("api/v1/brands")]
[Authorize]
public class BrandController : ControllerBase
{
    private readonly IBrandBL _brandBl;

    public BrandController(IBrandBL brandBl)
    {
        _brandBl = brandBl;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBrands(CancellationToken cancellationToken)
    {
        var result = await _brandBl.GetAllBrandsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrandById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _brandBl.GetBrandByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request, CancellationToken cancellationToken)
    {
        var result = await _brandBl.CreateBrandAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetBrandById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandRequest request, CancellationToken cancellationToken)
    {
        var result = await _brandBl.UpdateBrandAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
