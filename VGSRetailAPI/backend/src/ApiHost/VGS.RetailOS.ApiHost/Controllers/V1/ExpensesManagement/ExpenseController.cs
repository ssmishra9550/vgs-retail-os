using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.ExpensesManagement.Requests;
using VGS.RetailOS.Contracts.V1.ExpensesManagement.Responses;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.ExpensesManagement;

[ApiController]
[Route("api/v1/expenses")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseBL _expenseBl;

    public ExpenseController(IExpenseBL expenseBl)
    {
        _expenseBl = expenseBl;
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> RecordExpense([FromBody] RecordExpenseRequest request, CancellationToken cancellationToken)
    {
        var response = await _expenseBl.RecordExpenseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetExpenseById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseResponse>> GetExpenseById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _expenseBl.GetExpenseByIdAsync(id, cancellationToken);
        if (response == null) return NotFound();

        return Ok(response);
    }

    [HttpGet("store/{storeId:guid}")]
    public async Task<ActionResult<System.Collections.Generic.IEnumerable<ExpenseResponse>>> GetAllExpenses(Guid storeId, CancellationToken cancellationToken)
    {
        var response = await _expenseBl.GetAllExpensesAsync(storeId, cancellationToken);
        return Ok(response);
    }
}
