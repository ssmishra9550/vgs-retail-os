using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ReportsManagement.Responses;
using VGS.RetailOS.Modules.ReportsManagement.Report.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.ReportsManagement;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportBL _reportBl;

    public ReportController(IReportBL reportBl)
    {
        _reportBl = reportBl;
    }

    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetDashboardSummary(
        [FromQuery] Guid storeId, 
        [FromQuery] DateTimeOffset? startDate, 
        [FromQuery] DateTimeOffset? endDate, 
        CancellationToken cancellationToken)
    {
        var response = await _reportBl.GetDashboardSummaryAsync(storeId, startDate, endDate, cancellationToken);
        return Ok(response);
    }

    [HttpGet("dashboard/top-products")]
    public async Task<ActionResult<IEnumerable<TopProductResponse>>> GetTopProducts(
        [FromQuery] Guid storeId, 
        [FromQuery] DateTimeOffset? startDate, 
        [FromQuery] DateTimeOffset? endDate, 
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var response = await _reportBl.GetTopProductsAsync(storeId, startDate, endDate, limit, cancellationToken);
        return Ok(response);
    }

    [HttpGet("dashboard/low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockAlertResponse>>> GetLowStockAlerts(
        [FromQuery] Guid storeId, 
        [FromQuery] decimal threshold = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _reportBl.GetLowStockAlertsAsync(storeId, threshold, cancellationToken);
        return Ok(response);
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportResponse>> GetSalesReport(
        [FromQuery] Guid storeId, 
        [FromQuery] DateTimeOffset startDate, 
        [FromQuery] DateTimeOffset endDate, 
        CancellationToken cancellationToken)
    {
        var response = await _reportBl.GetSalesReportAsync(storeId, startDate, endDate, cancellationToken);
        return Ok(response);
    }

    [HttpGet("inventory/valuation")]
    public async Task<ActionResult<InventoryValuationResponse>> GetInventoryValuation(
        [FromQuery] Guid storeId, 
        CancellationToken cancellationToken)
    {
        var response = await _reportBl.GetInventoryValuationAsync(storeId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("financial/summary")]
    public async Task<ActionResult<FinancialSummaryResponse>> GetFinancialSummary(
        [FromQuery] Guid storeId, 
        [FromQuery] DateTimeOffset startDate, 
        [FromQuery] DateTimeOffset endDate, 
        CancellationToken cancellationToken)
    {
        var response = await _reportBl.GetFinancialSummaryAsync(storeId, startDate, endDate, cancellationToken);
        return Ok(response);
    }
}
