using System;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.PaymentsManagement.Requests;
using VGS.RetailOS.Contracts.V1.PaymentsManagement.Responses;

namespace VGS.RetailOS.Modules.PaymentsManagement.Payment.IBL;

public interface IPaymentBL
{
    Task<PaymentResponse> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken);
    Task<PaymentResponse?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<System.Collections.Generic.IEnumerable<PaymentResponse>> GetAllPaymentsAsync(Guid storeId, CancellationToken cancellationToken);
}
