using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.Audit.Requests;
using VGS.RetailOS.Contracts.V1.Audit.Responses;
using VGS.RetailOS.Modules.Audit.IBL;
using VGS.RetailOS.Shared.BuildingBlocks.Pagination;

namespace VGS.RetailOS.ApiHost.Controllers.V1;

[ApiController]
[Route("api/v1/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditBL _auditBl;

    public AuditController(IAuditBL auditBl)
    {
        _auditBl = auditBl;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditBl.GetAuditLogsAsync(request, cancellationToken);
        return Ok(result);
    }
}
