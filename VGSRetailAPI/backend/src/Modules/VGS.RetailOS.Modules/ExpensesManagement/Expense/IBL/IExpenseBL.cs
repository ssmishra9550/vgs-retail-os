using System;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ExpensesManagement.Requests;
using VGS.RetailOS.Contracts.V1.ExpensesManagement.Responses;

namespace VGS.RetailOS.Modules.ExpensesManagement.Expense.IBL;

public interface IExpenseBL
{
    Task<ExpenseResponse> RecordExpenseAsync(RecordExpenseRequest request, CancellationToken cancellationToken);
    Task<ExpenseResponse?> GetExpenseByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<System.Collections.Generic.IEnumerable<ExpenseResponse>> GetAllExpensesAsync(Guid storeId, CancellationToken cancellationToken);
}
