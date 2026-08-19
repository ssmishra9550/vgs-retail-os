using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.PaymentsManagement.Requests;
using VGS.RetailOS.Modules.CustomerManagement.Customer.IBL;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.BL;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.BO;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.IDAC;
using VGS.RetailOS.Modules.SupplierManagement.Supplier.IBL;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Modules.PaymentsManagement.Payment.BL;

public class PaymentBLTests
{
    private readonly Mock<IPaymentDAC> _paymentDacMock;
    private readonly Mock<ICustomerBL> _customerBlMock;
    private readonly Mock<ISupplierBL> _supplierBlMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly PaymentBL _paymentBl;

    private readonly string _tenantId = "tenant-123";
    private readonly Guid _storeId = Guid.NewGuid();
    private readonly Guid _referenceId = Guid.NewGuid();

    public PaymentBLTests()
    {
        _paymentDacMock = new Mock<IPaymentDAC>();
        _customerBlMock = new Mock<ICustomerBL>();
        _supplierBlMock = new Mock<ISupplierBL>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        var tenantContext = new TenantContext(_tenantId);
        _tenantContextAccessorMock.Setup(m => m.TenantContext).Returns(tenantContext);

        _paymentBl = new PaymentBL(_paymentDacMock.Object, _customerBlMock.Object, _supplierBlMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task RecordPaymentAsync_ZeroAmount_ThrowsValidationException()
    {
        var request = new RecordPaymentRequest
        {
            StoreId = _storeId,
            PaymentType = "CustomerReceipt",
            ReferenceId = _referenceId,
            Amount = 0,
            PaymentMethod = "Cash"
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _paymentBl.RecordPaymentAsync(request, CancellationToken.None));
        Assert.Equal("Payment amount must be greater than zero.", exception.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_InvalidPaymentType_ThrowsValidationException()
    {
        var request = new RecordPaymentRequest
        {
            StoreId = _storeId,
            PaymentType = "UnknownType",
            ReferenceId = _referenceId,
            Amount = 100,
            PaymentMethod = "Cash"
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _paymentBl.RecordPaymentAsync(request, CancellationToken.None));
        Assert.Equal("PaymentType must be either CustomerReceipt or SupplierPayment.", exception.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_CustomerReceipt_CallsCustomerBL()
    {
        var request = new RecordPaymentRequest
        {
            StoreId = _storeId,
            PaymentType = "CustomerReceipt",
            ReferenceId = _referenceId,
            Amount = 100,
            PaymentMethod = "Cash"
        };

        _paymentDacMock.Setup(x => x.RecordPaymentAsync(It.IsAny<PaymentBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentBO bo, CancellationToken _) =>
            {
                bo.Id = Guid.NewGuid();
                return bo;
            });

        var result = await _paymentBl.RecordPaymentAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        _paymentDacMock.Verify(x => x.RecordPaymentAsync(It.IsAny<PaymentBO>(), It.IsAny<CancellationToken>()), Times.Once);
        
        // Assert customer debt is reduced
        _customerBlMock.Verify(x => x.UpdateCreditBalanceAsync(_referenceId, -100m, It.IsAny<CancellationToken>()), Times.Once);
        _supplierBlMock.Verify(x => x.UpdateOutstandingPayableAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordPaymentAsync_SupplierPayment_CallsSupplierBL()
    {
        var request = new RecordPaymentRequest
        {
            StoreId = _storeId,
            PaymentType = "SupplierPayment",
            ReferenceId = _referenceId,
            Amount = 500,
            PaymentMethod = "BankTransfer"
        };

        _paymentDacMock.Setup(x => x.RecordPaymentAsync(It.IsAny<PaymentBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentBO bo, CancellationToken _) =>
            {
                bo.Id = Guid.NewGuid();
                return bo;
            });

        var result = await _paymentBl.RecordPaymentAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        _paymentDacMock.Verify(x => x.RecordPaymentAsync(It.IsAny<PaymentBO>(), It.IsAny<CancellationToken>()), Times.Once);
        
        // Assert supplier payable is reduced
        _supplierBlMock.Verify(x => x.UpdateOutstandingPayableAsync(_referenceId, -500m, It.IsAny<CancellationToken>()), Times.Once);
        _customerBlMock.Verify(x => x.UpdateCreditBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
