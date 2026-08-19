using System;

namespace VGS.RetailOS.Contracts.V1.ExpensesManagement.Requests;

public class RecordExpenseRequest
{
    public Guid StoreId { get; set; }
    public string Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTimeOffset ExpenseDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Description { get; set; } = null!;
}
