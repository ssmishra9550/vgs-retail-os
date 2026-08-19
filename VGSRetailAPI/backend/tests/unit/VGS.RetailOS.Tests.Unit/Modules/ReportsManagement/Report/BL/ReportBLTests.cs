using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VGS.RetailOS.Contracts.V1.ReportsManagement.Responses;
using VGS.RetailOS.Modules.ReportsManagement.Report.BL;
using VGS.RetailOS.Modules.ReportsManagement.Report.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Modules.ReportsManagement.Report.BL;

public class ReportBLTests
{
    private readonly Mock<IReportDAC> _reportDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly ReportBL _reportBl;

    private readonly string _tenantId = "tenant-123";
    private readonly Guid _storeId = Guid.NewGuid();

    public ReportBLTests()
    {
        _reportDacMock = new Mock<IReportDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        var tenantContext = new TenantContext(_tenantId);
        _tenantContextAccessorMock.Setup(m => m.TenantContext).Returns(tenantContext);

        _reportBl = new ReportBL(_reportDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_CallsDacWithTenantId()
    {
        var expectedResponse = new DashboardSummaryResponse { TodaySales = 1000m };
        _reportDacMock.Setup(x => x.GetDashboardSummaryAsync(_storeId, _tenantId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _reportBl.GetDashboardSummaryAsync(_storeId, null, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1000m, result.TodaySales);
        _reportDacMock.Verify(x => x.GetDashboardSummaryAsync(_storeId, _tenantId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFinancialSummaryAsync_CallsDacWithTenantId()
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-7);
        var endDate = DateTimeOffset.UtcNow;
        var expectedResponse = new FinancialSummaryResponse { GrossProfit = 500m };
        
        _reportDacMock.Setup(x => x.GetFinancialSummaryAsync(_storeId, _tenantId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _reportBl.GetFinancialSummaryAsync(_storeId, startDate, endDate, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(500m, result.GrossProfit);
        _reportDacMock.Verify(x => x.GetFinancialSummaryAsync(_storeId, _tenantId, startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_MissingTenantContext_ThrowsUnauthorizedException()
    {
        _tenantContextAccessorMock.Setup(m => m.TenantContext).Returns((TenantContext?)null);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _reportBl.GetDashboardSummaryAsync(_storeId, null, null, CancellationToken.None));
        
        Assert.Equal("Tenant context is missing.", exception.Message);
    }
}
