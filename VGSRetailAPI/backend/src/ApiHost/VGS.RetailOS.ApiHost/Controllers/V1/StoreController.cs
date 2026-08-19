using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.Store.Requests;
using VGS.RetailOS.Contracts.V1.Store.Responses;
using VGS.RetailOS.Modules.Store.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1;

[ApiController]
[Route("api/v1/stores")]
[Authorize] // Enforce authentication globally for this controller
public class StoreController : ControllerBase
{
    private readonly IStoreBL _storeBl;

    public StoreController(IStoreBL storeBl)
    {
        _storeBl = storeBl ?? throw new ArgumentNullException(nameof(storeBl));
    }

    [HttpPost]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateStoreRequest request, CancellationToken cancellationToken)
    {
        var response = await _storeBl.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateStoreRequest request, CancellationToken cancellationToken)
    {
        var response = await _storeBl.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _storeBl.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<StoreResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllStoresAsync([FromQuery] Guid? organizationId, CancellationToken cancellationToken)
    {
        if (organizationId.HasValue && organizationId.Value != Guid.Empty)
        {
            var response = await _storeBl.GetByOrganizationIdAsync(organizationId.Value, cancellationToken);
            return Ok(response);
        }
        else
        {
            var response = await _storeBl.GetAllForTenantAsync(cancellationToken);
            return Ok(response);
        }
    }
}
