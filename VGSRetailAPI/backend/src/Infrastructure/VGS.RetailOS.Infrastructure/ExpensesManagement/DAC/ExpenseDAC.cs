using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.ExpensesManagement.Entities;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.BO;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.IDAC;

namespace VGS.RetailOS.Infrastructure.ExpensesManagement.DAC;

public class ExpenseDAC : IExpenseDAC
{
    private readonly AppDbContext _context;

    public ExpenseDAC(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseBO> RecordExpenseAsync(ExpenseBO expense, CancellationToken cancellationToken)
    {
        var entity = new ExpenseEntity
        {
            Id = expense.Id == Guid.Empty ? Guid.NewGuid() : expense.Id,
            TenantId = expense.TenantId,
            StoreId = expense.StoreId,
            Category = expense.Category,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            PaymentMethod = expense.PaymentMethod,
            Description = expense.Description,
            Status = expense.Status
        };

        _context.Expenses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        expense.Id = entity.Id;
        return expense;
    }

    public async Task<ExpenseBO?> GetExpenseByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId, cancellationToken);

        if (entity == null) return null;

        return new ExpenseBO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            StoreId = entity.StoreId,
            Category = entity.Category,
            Amount = entity.Amount,
            ExpenseDate = entity.ExpenseDate,
            PaymentMethod = entity.PaymentMethod,
            Description = entity.Description,
            Status = entity.Status
        };
    }

    public async Task<System.Collections.Generic.IEnumerable<ExpenseBO>> GetAllExpensesAsync(Guid storeId, string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.StoreId == storeId && e.TenantId == tenantId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new ExpenseBO
        {
            Id = e.Id,
            TenantId = e.TenantId,
            StoreId = e.StoreId,
            Category = e.Category,
            Amount = e.Amount,
            ExpenseDate = e.ExpenseDate,
            PaymentMethod = e.PaymentMethod,
            Description = e.Description,
            Status = e.Status
        });
    }
}
