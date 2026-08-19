using System;

namespace VGS.RetailOS.Infrastructure.PaymentsManagement.Entities;

public class PaymentEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid StoreId { get; set; }
    
    public string PaymentType { get; set; } = null!; // CustomerReceipt, SupplierPayment
    public Guid ReferenceId { get; set; } // CustomerId or SupplierId
    
    public decimal Amount { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
