using System;

namespace VGS.RetailOS.Contracts.V1.ExpensesManagement.Responses;

public class ExpenseResponse
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTimeOffset ExpenseDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
}
