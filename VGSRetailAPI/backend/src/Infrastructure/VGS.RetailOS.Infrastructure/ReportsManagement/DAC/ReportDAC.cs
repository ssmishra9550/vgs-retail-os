using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ReportsManagement.Responses;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Modules.ReportsManagement.Report.IDAC;

namespace VGS.RetailOS.Infrastructure.ReportsManagement.DAC;

public class ReportDAC : IReportDAC
{
    private readonly AppDbContext _dbContext;

    public ReportDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid storeId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken)
    {
        var sales = await _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.StoreId == storeId && s.TenantId == tenantId && s.Status == "Completed" && s.SaleDate >= startDate && s.SaleDate <= endDate)
            .SumAsync(s => s.GrandTotal, cancellationToken);

        var purchases = await _dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.StoreId == storeId && p.TenantId == tenantId && p.Status == "Received" && p.InvoiceDate >= startDate && p.InvoiceDate <= endDate)
            .SumAsync(p => p.GrandTotal, cancellationToken);

        var expenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.StoreId == storeId && e.TenantId == tenantId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .SumAsync(e => e.Amount, cancellationToken);

        var receivables = await _dbContext.Customers
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .SumAsync(c => c.CreditBalance, cancellationToken);

        var payables = await _dbContext.Suppliers
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .SumAsync(s => s.OutstandingPayable, cancellationToken);

        return new DashboardSummaryResponse
        {
            TodaySales = sales,
            TodayPurchases = purchases,
            TotalExpensesThisMonth = expenses,
            TotalReceivables = receivables,
            TotalPayables = payables
        };
    }

    public async Task<IEnumerable<TopProductResponse>> GetTopProductsAsync(Guid storeId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, int limit, CancellationToken cancellationToken)
    {
        var topProducts = await _dbContext.SaleItems
            .AsNoTracking()
            .Where(si => si.Sale.StoreId == storeId && si.Sale.TenantId == tenantId && si.Sale.Status == "Completed" && si.Sale.SaleDate >= startDate && si.Sale.SaleDate <= endDate)
            .GroupBy(si => new { si.ProductId, si.Product.Name })
            .Select(g => new TopProductResponse
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalQuantitySold = g.Sum(si => si.Quantity),
                TotalRevenue = g.Sum(si => si.Total)
            })
            .OrderByDescending(r => r.TotalRevenue)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return topProducts;
    }

    public async Task<IEnumerable<LowStockAlertResponse>> GetLowStockAlertsAsync(Guid storeId, string tenantId, decimal threshold, CancellationToken cancellationToken)
    {
        var lowStock = await _dbContext.StockBalances
            .AsNoTracking()
            .Join(_dbContext.Products.AsNoTracking(),
                sb => sb.ProductId,
                p => p.Id,
                (sb, p) => new { Stock = sb, Product = p })
            .Where(x => x.Stock.StoreId == storeId && x.Stock.TenantId == tenantId && x.Stock.Quantity <= threshold)
            .Select(x => new LowStockAlertResponse
            {
                ProductId = x.Stock.ProductId,
                ProductName = x.Product.Name,
                CurrentStock = x.Stock.Quantity,
                ReorderLevel = threshold
            })
            .ToListAsync(cancellationToken);

        return lowStock;
    }

    public async Task<SalesReportResponse> GetSalesReportAsync(Guid storeId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken)
    {
        var salesData = await _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.StoreId == storeId && s.TenantId == tenantId && s.Status == "Completed" && s.SaleDate >= startDate && s.SaleDate <= endDate)
            .GroupBy(s => 1)
            .Select(g => new SalesReportResponse
            {
                TotalSales = g.Sum(s => s.SubTotal),
                TotalDiscounts = g.Sum(s => s.TotalDiscount),
                TotalTax = g.Sum(s => s.TotalTax),
                NetSales = g.Sum(s => s.GrandTotal),
                InvoiceCount = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var response = salesData ?? new SalesReportResponse();
        response.StartDate = startDate;
        response.EndDate = endDate;
        return response;
    }

    public async Task<InventoryValuationResponse> GetInventoryValuationAsync(Guid storeId, string tenantId, CancellationToken cancellationToken)
    {
        var products = await _dbContext.StockBalances
            .AsNoTracking()
            .Join(_dbContext.Products.AsNoTracking(),
                sb => sb.ProductId,
                p => p.Id,
                (sb, p) => new { Stock = sb, Product = p })
            .Where(x => x.Stock.StoreId == storeId && x.Stock.TenantId == tenantId && x.Stock.Quantity > 0)
            .Select(x => new ProductValuation
            {
                ProductId = x.Stock.ProductId,
                ProductName = x.Product.Name,
                CurrentStock = x.Stock.Quantity,
                PurchasePrice = x.Product.PurchasePrice, // Typically there is a specific purchase price vs selling price
                Valuation = x.Stock.Quantity * x.Product.PurchasePrice
            })
            .ToListAsync(cancellationToken);

        return new InventoryValuationResponse
        {
            TotalStockQuantity = products.Sum(p => p.CurrentStock),
            TotalValuation = products.Sum(p => p.Valuation),
            Products = products
        };
    }

    public async Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid storeId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken)
    {
        var sales = await _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.StoreId == storeId && s.TenantId == tenantId && s.Status == "Completed" && s.SaleDate >= startDate && s.SaleDate <= endDate)
            .SumAsync(s => s.SubTotal, cancellationToken);
            
        var discounts = await _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.StoreId == storeId && s.TenantId == tenantId && s.Status == "Completed" && s.SaleDate >= startDate && s.SaleDate <= endDate)
            .SumAsync(s => s.TotalDiscount, cancellationToken);

        var purchases = await _dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.StoreId == storeId && p.TenantId == tenantId && p.Status == "Received" && p.InvoiceDate >= startDate && p.InvoiceDate <= endDate)
            .SumAsync(p => p.GrandTotal, cancellationToken);

        var expenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.StoreId == storeId && e.TenantId == tenantId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .SumAsync(e => e.Amount, cancellationToken);

        return new FinancialSummaryResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            GrossSales = sales,
            TotalDiscounts = discounts,
            TotalPurchases = purchases,
            TotalExpenses = expenses,
            GrossProfit = sales - discounts - purchases - expenses
        };
    }
}
