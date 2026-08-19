using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ExpensesManagement.Requests;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.BL;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.BO;
using VGS.RetailOS.Modules.ExpensesManagement.Expense.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Modules.ExpensesManagement.Expense.BL;

public class ExpenseBLTests
{
    private readonly Mock<IExpenseDAC> _expenseDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly ExpenseBL _expenseBl;

    private readonly string _tenantId = "tenant-123";
    private readonly Guid _storeId = Guid.NewGuid();

    public ExpenseBLTests()
    {
        _expenseDacMock = new Mock<IExpenseDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        var tenantContext = new TenantContext(_tenantId);
        _tenantContextAccessorMock.Setup(m => m.TenantContext).Returns(tenantContext);

        _expenseBl = new ExpenseBL(_expenseDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task RecordExpenseAsync_ZeroAmount_ThrowsValidationException()
    {
        var request = new RecordExpenseRequest
        {
            StoreId = _storeId,
            Category = "Rent",
            Amount = 0,
            ExpenseDate = DateTimeOffset.UtcNow,
            PaymentMethod = "BankTransfer",
            Description = "Monthly Rent"
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _expenseBl.RecordExpenseAsync(request, CancellationToken.None));
        Assert.Equal("Expense amount must be greater than zero.", exception.Message);
    }

    [Fact]
    public async Task RecordExpenseAsync_ValidRequest_SavesExpense()
    {
        var request = new RecordExpenseRequest
        {
            StoreId = _storeId,
            Category = "Salary",
            Amount = 5000,
            ExpenseDate = DateTimeOffset.UtcNow,
            PaymentMethod = "BankTransfer",
            Description = "Staff Salary"
        };

        _expenseDacMock.Setup(x => x.RecordExpenseAsync(It.IsAny<ExpenseBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExpenseBO bo, CancellationToken _) =>
            {
                bo.Id = Guid.NewGuid();
                return bo;
            });

        var result = await _expenseBl.RecordExpenseAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Salary", result.Category);
        Assert.Equal("Approved", result.Status); // Defaults to Approved for MVP
        _expenseDacMock.Verify(x => x.RecordExpenseAsync(It.IsAny<ExpenseBO>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
