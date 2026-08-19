using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ReportsManagement.Responses;

namespace VGS.RetailOS.Modules.ReportsManagement.Report.IBL;

public interface IReportBL
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid storeId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken);
    Task<IEnumerable<TopProductResponse>> GetTopProductsAsync(Guid storeId, DateTimeOffset? startDate, DateTimeOffset? endDate, int limit, CancellationToken cancellationToken);
    Task<IEnumerable<LowStockAlertResponse>> GetLowStockAlertsAsync(Guid storeId, decimal threshold, CancellationToken cancellationToken);
    
    Task<SalesReportResponse> GetSalesReportAsync(Guid storeId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken);
    Task<InventoryValuationResponse> GetInventoryValuationAsync(Guid storeId, CancellationToken cancellationToken);
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid storeId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken);
}
