using System;

namespace VGS.RetailOS.Contracts.V1.PaymentsManagement.Responses;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string PaymentType { get; set; } = null!;
    public Guid ReferenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
