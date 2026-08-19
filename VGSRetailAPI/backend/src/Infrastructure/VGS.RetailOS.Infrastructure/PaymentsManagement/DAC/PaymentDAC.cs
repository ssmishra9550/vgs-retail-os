using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.PaymentsManagement.Entities;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.BO;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.IDAC;

namespace VGS.RetailOS.Infrastructure.PaymentsManagement.DAC;

public class PaymentDAC : IPaymentDAC
{
    private readonly AppDbContext _context;

    public PaymentDAC(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentBO> RecordPaymentAsync(PaymentBO payment, CancellationToken cancellationToken)
    {
        var entity = new PaymentEntity
        {
            Id = payment.Id == Guid.Empty ? Guid.NewGuid() : payment.Id,
            TenantId = payment.TenantId,
            StoreId = payment.StoreId,
            PaymentType = payment.PaymentType,
            ReferenceId = payment.ReferenceId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes
        };

        _context.Payments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        payment.Id = entity.Id;
        return payment;
    }

    public async Task<PaymentBO?> GetPaymentByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);

        if (entity == null) return null;

        return new PaymentBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            StoreId = entity.StoreId,
            PaymentType = entity.PaymentType,
            ReferenceId = entity.ReferenceId,
            Amount = entity.Amount,
            PaymentDate = entity.PaymentDate,
            PaymentMethod = entity.PaymentMethod,
            ReferenceNumber = entity.ReferenceNumber,
            Notes = entity.Notes
        };
    }

    public async Task<System.Collections.Generic.IEnumerable<PaymentBO>> GetAllPaymentsAsync(Guid storeId, string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _context.Payments
            .AsNoTracking()
            .Where(p => p.StoreId == storeId && p.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new PaymentBO
        {
            Id = e.Id,
            TenantId = e.TenantId,
            StoreId = e.StoreId,
            PaymentType = e.PaymentType,
            ReferenceId = e.ReferenceId,
            Amount = e.Amount,
            PaymentDate = e.PaymentDate,
            PaymentMethod = e.PaymentMethod,
            ReferenceNumber = e.ReferenceNumber,
            Notes = e.Notes
        });
    }
}
