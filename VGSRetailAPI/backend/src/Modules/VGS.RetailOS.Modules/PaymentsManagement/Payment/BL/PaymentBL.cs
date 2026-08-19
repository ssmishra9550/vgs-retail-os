using System;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.PaymentsManagement.Requests;
using VGS.RetailOS.Contracts.V1.PaymentsManagement.Responses;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IBL;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.BO;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.IDAC;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.IBL;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IBL;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.PaymentsManagement.Payment.BL;

public class PaymentBL : IPaymentBL
{
    private readonly IPaymentDAC _paymentDac;
    private readonly ICustomerBL _customerBl;
    private readonly ISupplierBL _supplierBl;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public PaymentBL(
        IPaymentDAC paymentDac, 
        ICustomerBL customerBl, 
        ISupplierBL supplierBl, 
        ITenantContextAccessor tenantContextAccessor)
    {
        _paymentDac = paymentDac;
        _customerBl = customerBl;
        _supplierBl = supplierBl;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<PaymentResponse> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId 
            ?? throw new UnauthorizedAccessException("Tenant context is required.");

        if (request.Amount <= 0)
            throw new ValidationException("Payment amount must be greater than zero.");

        if (request.PaymentType != "CustomerReceipt" && request.PaymentType != "SupplierPayment")
            throw new ValidationException("PaymentType must be either CustomerReceipt or SupplierPayment.");

        var bo = new PaymentBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StoreId = request.StoreId,
            PaymentType = request.PaymentType,
            ReferenceId = request.ReferenceId,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            PaymentMethod = request.PaymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes
        };

        var savedBo = await _paymentDac.RecordPaymentAsync(bo, cancellationToken);

        if (request.PaymentType == "CustomerReceipt")
        {
            // A receipt from a customer reduces their credit balance (debt)
            await _customerBl.UpdateCreditBalanceAsync(request.ReferenceId, -request.Amount, cancellationToken);
        }
        else if (request.PaymentType == "SupplierPayment")
        {
            // A payment to a supplier reduces our outstanding payable (debt)
            await _supplierBl.UpdateOutstandingPayableAsync(request.ReferenceId, -request.Amount, cancellationToken);
        }

        return MapToResponse(savedBo);
    }

    public async Task<PaymentResponse?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId 
            ?? throw new UnauthorizedAccessException("Tenant context is required.");

        var bo = await _paymentDac.GetPaymentByIdAsync(id, tenantId, cancellationToken);
        if (bo == null) return null;

        return MapToResponse(bo);
    }

    public async Task<System.Collections.Generic.IEnumerable<PaymentResponse>> GetAllPaymentsAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId 
            ?? throw new UnauthorizedAccessException("Tenant context is required.");

        var bos = await _paymentDac.GetAllPaymentsAsync(storeId, tenantId, cancellationToken);
        return System.Linq.Enumerable.Select(bos, MapToResponse);
    }

    private PaymentResponse MapToResponse(PaymentBO bo)
    {
        return new PaymentResponse
        {
            Id = bo.Id,
            StoreId = bo.StoreId,
            PaymentType = bo.PaymentType,
            ReferenceId = bo.ReferenceId,
            Amount = bo.Amount,
            PaymentDate = bo.PaymentDate,
            PaymentMethod = bo.PaymentMethod,
            ReferenceNumber = bo.ReferenceNumber,
            Notes = bo.Notes
        };
    }
}
