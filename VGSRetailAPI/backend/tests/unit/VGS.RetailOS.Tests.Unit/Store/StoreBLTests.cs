using Moq;
using VGS.RetailOS.Contracts.V1.Store.Requests;
using VGS.RetailOS.Modules.Organization.BO;
using VGS.RetailOS.Modules.Organization.IDAC;
using VGS.RetailOS.Modules.Store.BL;
using VGS.RetailOS.Modules.Store.BO;
using VGS.RetailOS.Modules.Store.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Tests.Unit.Store;

public class StoreBLTests
{
    private readonly Mock<IStoreDAC> _storeDacMock;
    private readonly Mock<IOrganizationDAC> _orgDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly StoreBL _bl;
    private readonly string _tenantId = "tenant-1";

    public StoreBLTests()
    {
        _storeDacMock = new Mock<IStoreDAC>();
        _orgDacMock = new Mock<IOrganizationDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        var tenantContext = new TenantContext(_tenantId);
        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(tenantContext);

        _bl = new StoreBL(_storeDacMock.Object, _orgDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenOrganizationNotFound_ThrowsNotFoundException()
    {
        var request = new CreateStoreRequest { Name = "New Store", OrganizationId = Guid.NewGuid() };
        
        _orgDacMock.Setup(x => x.GetByIdAsync(request.OrganizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationBO?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _bl.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenOrganizationBelongsToDifferentTenant_ThrowsNotFoundException()
    {
        var request = new CreateStoreRequest { Name = "New Store", OrganizationId = Guid.NewGuid() };
        var orgBo = new OrganizationBO { Id = request.OrganizationId, TenantId = "different-tenant", Name = "Org" };
        
        _orgDacMock.Setup(x => x.GetByIdAsync(request.OrganizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgBo);

        await Assert.ThrowsAsync<NotFoundException>(() => _bl.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenNameExistsInOrg_ThrowsConflictException()
    {
        var request = new CreateStoreRequest { Name = "Existing Store", OrganizationId = Guid.NewGuid() };
        var orgBo = new OrganizationBO { Id = request.OrganizationId, TenantId = _tenantId, Name = "Org" };
        
        _orgDacMock.Setup(x => x.GetByIdAsync(request.OrganizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgBo);
            
        _storeDacMock.Setup(x => x.ExistsByNameAsync("Existing Store", request.OrganizationId, _tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _bl.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenValid_ReturnsStoreResponse()
    {
        var request = new CreateStoreRequest { Name = "New Store", OrganizationId = Guid.NewGuid() };
        var orgBo = new OrganizationBO { Id = request.OrganizationId, TenantId = _tenantId, Name = "Org" };
        
        _orgDacMock.Setup(x => x.GetByIdAsync(request.OrganizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgBo);
            
        _storeDacMock.Setup(x => x.ExistsByNameAsync("New Store", request.OrganizationId, _tenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _storeDacMock.Setup(x => x.CreateAsync(It.IsAny<StoreBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreBO bo, CancellationToken _) => bo);

        var result = await _bl.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("New Store", result.Name);
        Assert.Equal(request.OrganizationId, result.OrganizationId);
    }
}
