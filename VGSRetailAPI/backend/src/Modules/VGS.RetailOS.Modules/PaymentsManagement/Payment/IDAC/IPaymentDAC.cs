using System;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.BO;

namespace VGS.RetailOS.Modules.PaymentsManagement.Payment.IDAC;

public interface IPaymentDAC
{
    Task<PaymentBO> RecordPaymentAsync(PaymentBO payment, CancellationToken cancellationToken);
    Task<PaymentBO?> GetPaymentByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<System.Collections.Generic.IEnumerable<PaymentBO>> GetAllPaymentsAsync(Guid storeId, string tenantId, CancellationToken cancellationToken);
}
