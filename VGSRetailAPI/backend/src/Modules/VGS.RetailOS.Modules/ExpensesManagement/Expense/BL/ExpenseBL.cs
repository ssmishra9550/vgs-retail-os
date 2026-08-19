using System;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ExpensesManagement.Requests;
using VGS.RetailOS.Contracts.V1.ExpensesManagement.Responses;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.BO;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.IDAC;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.IBL;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.ExpensesManagement.Expense.BL;

public class ExpenseBL : IExpenseBL
{
    private readonly IExpenseDAC _expenseDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public ExpenseBL(IExpenseDAC expenseDac, ITenantContextAccessor tenantContextAccessor)
    {
        _expenseDac = expenseDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<ExpenseResponse> RecordExpenseAsync(RecordExpenseRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId 
            ?? throw new UnauthorizedAccessException("Tenant context is required.");

        if (request.Amount <= 0)
            throw new ValidationException("Expense amount must be greater than zero.");

        var bo = new ExpenseBO
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StoreId = request.StoreId,
            Category = request.Category,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate,
            PaymentMethod = request.PaymentMethod,
            Description = request.Description,
            Status = "Approved" // Defaulting to Approved for MVP
        };

        var savedBo = await _expenseDac.RecordExpenseAsync(bo, cancellationToken);

        return MapToResponse(savedBo);
    }

    public async Task<ExpenseResponse?> GetExpenseByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId 
            ?? throw new UnauthorizedAccessException("Tenant context is required.");

        var bo = await _expenseDac.GetExpenseByIdAsync(id, tenantId, cancellationToken);
        if (bo == null) return null;

        return MapToResponse(bo);
    }

    public async Task<System.Collections.Generic.IEnumerable<ExpenseResponse>> GetAllExpensesAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId 
            ?? throw new UnauthorizedAccessException("Tenant context is required.");

        var bos = await _expenseDac.GetAllExpensesAsync(storeId, tenantId, cancellationToken);
        return System.Linq.Enumerable.Select(bos, MapToResponse);
    }

    private ExpenseResponse MapToResponse(ExpenseBO bo)
    {
        return new ExpenseResponse
        {
            Id = bo.Id,
            StoreId = bo.StoreId,
            Category = bo.Category,
            Amount = bo.Amount,
            ExpenseDate = bo.ExpenseDate,
            PaymentMethod = bo.PaymentMethod,
            Description = bo.Description,
            Status = bo.Status
        };
    }
}
