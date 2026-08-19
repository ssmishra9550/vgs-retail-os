using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ReportsManagement.Responses;

namespace VGS.RetailOS.Modules.ReportsManagement.Report.IDAC;

public interface IReportDAC
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid storeId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken);
    Task<IEnumerable<TopProductResponse>> GetTopProductsAsync(Guid storeId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, int limit, CancellationToken cancellationToken);
    Task<IEnumerable<LowStockAlertResponse>> GetLowStockAlertsAsync(Guid storeId, string tenantId, decimal threshold, CancellationToken cancellationToken);
    
    Task<SalesReportResponse> GetSalesReportAsync(Guid storeId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken);
    Task<InventoryValuationResponse> GetInventoryValuationAsync(Guid storeId, string tenantId, CancellationToken cancellationToken);
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid storeId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken);
}
