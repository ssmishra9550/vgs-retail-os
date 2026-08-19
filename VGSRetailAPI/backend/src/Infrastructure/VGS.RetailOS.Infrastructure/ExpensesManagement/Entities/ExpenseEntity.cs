using System;

namespace VGS.RetailOS.Infrastructure.ExpensesManagement.Entities;

public class ExpenseEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid StoreId { get; set; }
    
    public string Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTimeOffset ExpenseDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!; // Draft, Approved
}
