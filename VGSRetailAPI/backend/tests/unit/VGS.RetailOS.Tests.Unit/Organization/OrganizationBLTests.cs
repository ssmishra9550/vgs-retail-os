using Moq;
using VGS.RetailOS.Contracts.V1.Organization.Requests;
using VGS.RetailOS.Modules.Organization.BL;
using VGS.RetailOS.Modules.Organization.BO;
using VGS.RetailOS.Modules.Organization.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Organization;

public class OrganizationBLTests
{
    private readonly Mock<IOrganizationDAC> _dacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly OrganizationBL _bl;

    public OrganizationBLTests()
    {
        _dacMock = new Mock<IOrganizationDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        var tenantContext = new TenantContext("tenant-1");
        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(tenantContext);

        _bl = new OrganizationBL(_dacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ThrowsValidationException()
    {
        var request = new CreateOrganizationRequest { Name = "" };

        await Assert.ThrowsAsync<ValidationException>(() => _bl.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsConflictException()
    {
        var request = new CreateOrganizationRequest { Name = "Existing Org" };
        _dacMock.Setup(x => x.ExistsByNameAsync("Existing Org", "tenant-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _bl.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenValid_ReturnsOrganizationResponse()
    {
        var request = new CreateOrganizationRequest { Name = "New Org", Code = "ORG1" };
        
        _dacMock.Setup(x => x.ExistsByNameAsync("New Org", "tenant-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dacMock.Setup(x => x.CreateAsync(It.IsAny<OrganizationBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationBO bo, CancellationToken _) => bo);

        var result = await _bl.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("New Org", result.Name);
        Assert.Equal("ORG1", result.Code);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _dacMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationBO?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _bl.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesAndReturns()
    {
        var id = Guid.NewGuid();
        var request = new UpdateOrganizationRequest { Name = "Updated Org" };
        var existingBo = new OrganizationBO { Id = id, TenantId = "tenant-1", Name = "Old Org" };

        _dacMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBo);

        _dacMock.Setup(x => x.ExistsByNameAsync("Updated Org", "tenant-1", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dacMock.Setup(x => x.UpdateAsync(It.IsAny<OrganizationBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationBO bo, CancellationToken _) => bo);

        var result = await _bl.UpdateAsync(id, request);

        Assert.NotNull(result);
        Assert.Equal("Updated Org", result.Name);
        Assert.NotNull(result.UpdatedAt);
    }
}
