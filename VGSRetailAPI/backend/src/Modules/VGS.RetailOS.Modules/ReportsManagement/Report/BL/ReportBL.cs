using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ReportsManagement.Responses;
using VGS.RetailOS.Modules.ReportsManagement.Report.IBL;
using VGS.RetailOS.Modules.ReportsManagement.Report.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.ReportsManagement.Report.BL;

public class ReportBL : IReportBL
{
    private readonly IReportDAC _reportDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public ReportBL(IReportDAC reportDac, ITenantContextAccessor tenantContextAccessor)
    {
        _reportDac = reportDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid storeId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        
        var start = startDate ?? DateTimeOffset.UtcNow.Date;
        var end = endDate ?? DateTimeOffset.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await _reportDac.GetDashboardSummaryAsync(storeId, tenantId, start, end, cancellationToken);
    }

    public async Task<IEnumerable<TopProductResponse>> GetTopProductsAsync(Guid storeId, DateTimeOffset? startDate, DateTimeOffset? endDate, int limit, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;
        var limitVal = limit > 0 ? limit : 5;

        return await _reportDac.GetTopProductsAsync(storeId, tenantId, start, end, limitVal, cancellationToken);
    }

    public async Task<IEnumerable<LowStockAlertResponse>> GetLowStockAlertsAsync(Guid storeId, decimal threshold, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var thresholdVal = threshold > 0 ? threshold : 10;

        return await _reportDac.GetLowStockAlertsAsync(storeId, tenantId, thresholdVal, cancellationToken);
    }

    public async Task<SalesReportResponse> GetSalesReportAsync(Guid storeId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        return await _reportDac.GetSalesReportAsync(storeId, tenantId, startDate, endDate, cancellationToken);
    }

    public async Task<InventoryValuationResponse> GetInventoryValuationAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        return await _reportDac.GetInventoryValuationAsync(storeId, tenantId, cancellationToken);
    }

    public async Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid storeId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        return await _reportDac.GetFinancialSummaryAsync(storeId, tenantId, startDate, endDate, cancellationToken);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new UnauthorizedException("Tenant context is missing.");
        }
        return tenantId;
    }
}
