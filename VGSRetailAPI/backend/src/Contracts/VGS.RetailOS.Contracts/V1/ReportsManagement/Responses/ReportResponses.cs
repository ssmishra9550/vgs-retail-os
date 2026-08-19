using System;
using System.Collections.Generic;

namespace VGS.RetailOS.Contracts.V1.ReportsManagement.Responses;

public class DashboardSummaryResponse
{
    public decimal TodaySales { get; set; }
    public decimal TodayPurchases { get; set; }
    public decimal TotalExpensesThisMonth { get; set; }
    public decimal TotalReceivables { get; set; }
    public decimal TotalPayables { get; set; }
}

public class TopProductResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class LowStockAlertResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
}

public class SalesReportResponse
{
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetSales { get; set; }
    public int InvoiceCount { get; set; }
}

public class InventoryValuationResponse
{
    public decimal TotalStockQuantity { get; set; }
    public decimal TotalValuation { get; set; } // Current stock * Purchase Price
    public List<ProductValuation> Products { get; set; } = new();
}

public class ProductValuation
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Valuation { get; set; }
}

public class FinancialSummaryResponse
{
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal GrossSales { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrossProfit { get; set; } // GrossSales - TotalDiscounts - TotalPurchases - TotalExpenses
}
