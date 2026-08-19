using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.PaymentsManagement.Requests;
using VGS.RetailOS.Contracts.V1.PaymentsManagement.Responses;
using VGS.RetailOS.Modules.PaymentsManagement.Payment.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.PaymentsManagement;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentBL _paymentBl;

    public PaymentController(IPaymentBL paymentBl)
    {
        _paymentBl = paymentBl;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> RecordPayment([FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var response = await _paymentBl.RecordPaymentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPaymentById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse>> GetPaymentById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _paymentBl.GetPaymentByIdAsync(id, cancellationToken);
        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet("store/{storeId:guid}")]
    public async Task<ActionResult<System.Collections.Generic.IEnumerable<PaymentResponse>>> GetAllPayments(Guid storeId, CancellationToken cancellationToken)
    {
        var response = await _paymentBl.GetAllPaymentsAsync(storeId, cancellationToken);
        return Ok(response);
    }
}
