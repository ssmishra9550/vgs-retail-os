using System;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.BO;

namespace VGS.RetailOS.Modules.ExpensesManagement.Expense.IDAC;

public interface IExpenseDAC
{
    Task<ExpenseBO> RecordExpenseAsync(ExpenseBO expense, CancellationToken cancellationToken);
    Task<ExpenseBO?> GetExpenseByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<System.Collections.Generic.IEnumerable<ExpenseBO>> GetAllExpensesAsync(Guid storeId, string tenantId, CancellationToken cancellationToken);
}
