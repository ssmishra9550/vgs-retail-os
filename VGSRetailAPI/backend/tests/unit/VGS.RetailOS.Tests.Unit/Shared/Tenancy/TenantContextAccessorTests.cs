using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Shared.Tenancy;

public class TenantContextAccessorTests
{
    [Fact]
    public void TenantContextAccessor_ShouldStoreAndRetrieveContext()
    {
        // Arrange
        var accessor = new TenantContextAccessor();
        var context = new TenantContext("tenant-123");

        // Act
        accessor.TenantContext = context;

        // Assert
        Assert.NotNull(accessor.TenantContext);
        Assert.Equal("tenant-123", accessor.TenantContext!.CurrentTenantId);
        Assert.True(accessor.TenantContext.IsTenantResolved);
    }

    [Fact]
    public void TenantContextAccessor_ShouldClearContext()
    {
        // Arrange
        var accessor = new TenantContextAccessor();
        accessor.TenantContext = new TenantContext("tenant-123");

        // Act
        accessor.TenantContext = null;

        // Assert
        Assert.Null(accessor.TenantContext);
    }

    [Fact]
    public async Task TenantContextAccessor_ShouldMaintainContextAcrossAsyncFlows()
    {
        // Arrange
        var accessor = new TenantContextAccessor();

        // Act
        accessor.TenantContext = new TenantContext("tenant-main");

        string? taskTenantId = null;

        await Task.Run(async () =>
        {
            // Context should flow into the background task
            taskTenantId = accessor.TenantContext?.CurrentTenantId;
            
            // Wait to ensure we don't return before main context checks
            await Task.Delay(10);
        });

        // Assert
        Assert.Equal("tenant-main", taskTenantId);
        Assert.Equal("tenant-main", accessor.TenantContext?.CurrentTenantId);
    }

    [Fact]
    public void TenantContext_ShouldThrowArgumentException_WhenTenantIdIsNullOrWhitespace()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TenantContext(""));
        Assert.Throws<ArgumentException>(() => new TenantContext("   "));
        Assert.Throws<ArgumentException>(() => new TenantContext(null!));
    }
}
