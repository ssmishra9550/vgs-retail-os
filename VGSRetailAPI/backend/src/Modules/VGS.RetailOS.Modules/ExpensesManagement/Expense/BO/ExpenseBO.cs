using System;

namespace VGS.RetailOS.Modules.ExpensesManagement.Expense.BO;

public class ExpenseBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid StoreId { get; set; }
    
    public string Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTimeOffset ExpenseDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
}
