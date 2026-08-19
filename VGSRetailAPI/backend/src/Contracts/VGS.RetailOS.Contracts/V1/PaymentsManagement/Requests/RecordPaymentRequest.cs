using System;

namespace VGS.RetailOS.Contracts.V1.PaymentsManagement.Requests;

public class RecordPaymentRequest
{
    public Guid StoreId { get; set; }
    public string PaymentType { get; set; } = null!; // CustomerReceipt, SupplierPayment
    public Guid ReferenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
