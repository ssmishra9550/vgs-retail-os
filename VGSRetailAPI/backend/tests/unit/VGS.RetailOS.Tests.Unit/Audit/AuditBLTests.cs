using Moq;
using VGS.RetailOS.Contracts.V1.Audit.Requests;
using VGS.RetailOS.Modules.Audit.BL;
using VGS.RetailOS.Modules.Audit.BO;
using VGS.RetailOS.Modules.Audit.IDAC;
using VGS.RetailOS.Shared.Auth;
using VGS.RetailOS.Shared.BuildingBlocks.Pagination;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Audit;

public class AuditBLTests
{
    private readonly Mock<IAuditDAC> _auditDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly Mock<IUserContextAccessor> _userContextAccessorMock;
    private readonly AuditBL _sut;

    public AuditBLTests()
    {
        _auditDacMock = new Mock<IAuditDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();
        _userContextAccessorMock = new Mock<IUserContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));
        _userContextAccessorMock.Setup(x => x.CurrentUserId).Returns(Guid.NewGuid());

        _sut = new AuditBL(_auditDacMock.Object, _tenantContextAccessorMock.Object, _userContextAccessorMock.Object);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldThrowUnauthorizedException_WhenTenantContextIsMissing()
    {
        // Arrange
        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns((TenantContext?)null);
        var request = new GetAuditLogsRequest();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.GetAuditLogsAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldReturnPaginatedList()
    {
        // Arrange
        var request = new GetAuditLogsRequest { PageNumber = 1, PageSize = 10 };
        var logs = new List<AuditLogBO>
        {
            new AuditLogBO { Id = Guid.NewGuid(), TenantId = "tenant-1", Action = "Create", EntityType = "Product", EntityId = "prod-1", Timestamp = DateTimeOffset.UtcNow }
        };
        var paginatedLogs = new PaginatedList<AuditLogBO>(logs, 1, 1, 10);

        _auditDacMock.Setup(x => x.GetAuditLogsAsync(request, "tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedLogs);

        // Act
        var result = await _sut.GetAuditLogsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Create", result.Items.First().Action);
        Assert.Equal("prod-1", result.Items.First().EntityId);
    }

    [Fact]
    public async Task LogBusinessEventAsync_ShouldCreateAuditLog()
    {
        // Arrange
        var action = "ApproveTransfer";
        var entityType = "StockTransfer";
        var entityId = "transfer-123";
        var reason = "Approved by manager";
        var correlationId = "corr-1";

        // Act
        await _sut.LogBusinessEventAsync(action, entityType, entityId, reason, null, null, correlationId, CancellationToken.None);

        // Assert
        _auditDacMock.Verify(x => x.CreateAuditLogAsync(It.Is<AuditLogBO>(a => 
            a.Action == action &&
            a.EntityType == entityType &&
            a.EntityId == entityId &&
            a.Reason == reason &&
            a.CorrelationId == correlationId &&
            a.TenantId == "tenant-1"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
