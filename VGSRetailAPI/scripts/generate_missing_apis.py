import os

BASE = "/Users/sauravmishra/VGSRetail/VGSRetailAPI/backend/src"
MOD_INV = f"{BASE}/Modules/VGS.RetailOS.Modules/InventoryManagement/StockTransfer"
INF_INV = f"{BASE}/Infrastructure/VGS.RetailOS.Infrastructure/InventoryManagement/DAC"
MOD_RET = f"{BASE}/Modules/VGS.RetailOS.Modules/ReturnsManagement/Return"
INF_RET = f"{BASE}/Infrastructure/VGS.RetailOS.Infrastructure/ReturnsManagement/DAC"
CTRL_INV = f"{BASE}/ApiHost/VGS.RetailOS.ApiHost/Controllers/V1/InventoryManagement"
CTRL_RET = f"{BASE}/ApiHost/VGS.RetailOS.ApiHost/Controllers/V1/ReturnsManagement"

os.makedirs(f"{MOD_INV}/BO", exist_ok=True)
os.makedirs(f"{MOD_INV}/IDAC", exist_ok=True)
os.makedirs(f"{MOD_INV}/IBL", exist_ok=True)
os.makedirs(f"{MOD_INV}/BL", exist_ok=True)
os.makedirs(INF_INV, exist_ok=True)
os.makedirs(f"{MOD_RET}/BO", exist_ok=True)
os.makedirs(f"{MOD_RET}/IDAC", exist_ok=True)
os.makedirs(f"{MOD_RET}/IBL", exist_ok=True)
os.makedirs(f"{MOD_RET}/BL", exist_ok=True)
os.makedirs(INF_RET, exist_ok=True)
os.makedirs(CTRL_INV, exist_ok=True)
os.makedirs(CTRL_RET, exist_ok=True)

# ----------------- STOCK TRANSFERS -----------------
with open(f"{MOD_INV}/BO/StockTransferBO.cs", "w") as f:
    f.write("""namespace VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
public class StockTransferBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string TransferNumber { get; set; } = null!;
    public Guid SourceStoreId { get; set; }
    public Guid DestinationStoreId { get; set; }
    public string Status { get; set; } = "Initiated";
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
}
""")

with open(f"{MOD_INV}/IDAC/IStockTransferDAC.cs", "w") as f:
    f.write("""using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
namespace VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IDAC;
public interface IStockTransferDAC
{
    Task<StockTransferBO> CreateTransferAsync(StockTransferBO transfer, CancellationToken cancellationToken);
    Task<StockTransferBO?> GetTransferByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<List<StockTransferBO>> GetAllTransfersAsync(string tenantId, CancellationToken cancellationToken);
}
""")

with open(f"{INF_INV}/StockTransferDAC.cs", "w") as f:
    f.write("""using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IDAC;

namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC;
public class StockTransferDAC : IStockTransferDAC
{
    private readonly AppDbContext _dbContext;
    public StockTransferDAC(AppDbContext dbContext) { _dbContext = dbContext; }

    public async Task<StockTransferBO> CreateTransferAsync(StockTransferBO transfer, CancellationToken cancellationToken)
    {
        var entity = new StockTransferEntity {
            Id = transfer.Id, TenantId = transfer.TenantId, TransferNumber = transfer.TransferNumber,
            SourceStoreId = transfer.SourceStoreId, DestinationStoreId = transfer.DestinationStoreId,
            Status = transfer.Status, ShippedAt = transfer.ShippedAt, ReceivedAt = transfer.ReceivedAt
        };
        _dbContext.StockTransfers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return transfer;
    }

    public async Task<StockTransferBO?> GetTransferByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.StockTransfers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity == null) return null;
        return new StockTransferBO { Id = entity.Id, TenantId = entity.TenantId, TransferNumber = entity.TransferNumber, SourceStoreId = entity.SourceStoreId, DestinationStoreId = entity.DestinationStoreId, Status = entity.Status };
    }

    public async Task<List<StockTransferBO>> GetAllTransfersAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.StockTransfers.AsNoTracking().Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken);
        return entities.Select(e => new StockTransferBO { Id = e.Id, TenantId = e.TenantId, TransferNumber = e.TransferNumber, SourceStoreId = e.SourceStoreId, DestinationStoreId = e.DestinationStoreId, Status = e.Status }).ToList();
    }
}
""")

with open(f"{MOD_INV}/IBL/IStockTransferBL.cs", "w") as f:
    f.write("""using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
namespace VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IBL;
public interface IStockTransferBL
{
    Task<StockTransferBO> InitiateTransferAsync(InitiateStockTransferRequest request, CancellationToken cancellationToken);
    Task<StockTransferBO> GetTransferAsync(Guid id, CancellationToken cancellationToken);
    Task<List<StockTransferBO>> GetAllTransfersAsync(CancellationToken cancellationToken);
}
""")

with open(f"{MOD_INV}/BL/StockTransferBL.cs", "w") as f:
    f.write("""using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BO;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IBL;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IDAC;
using VGS.RetailOS.Shared.Tenancy;
using VGS.RetailOS.Shared.Errors.Exceptions;
namespace VGS.RetailOS.Modules.InventoryManagement.StockTransfer.BL;
public class StockTransferBL : IStockTransferBL
{
    private readonly IStockTransferDAC _dac;
    private readonly ITenantContextAccessor _tenantAccessor;
    public StockTransferBL(IStockTransferDAC dac, ITenantContextAccessor tenantAccessor) { _dac = dac; _tenantAccessor = tenantAccessor; }

    public async Task<StockTransferBO> InitiateTransferAsync(InitiateStockTransferRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        var bo = new StockTransferBO {
            Id = Guid.NewGuid(), TenantId = tenantId, TransferNumber = $"TRN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            SourceStoreId = request.SourceStoreId, DestinationStoreId = request.DestinationStoreId,
            Status = "Initiated", ShippedAt = DateTimeOffset.UtcNow
        };
        return await _dac.CreateTransferAsync(bo, cancellationToken);
    }
    public async Task<StockTransferBO> GetTransferAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        return await _dac.GetTransferByIdAsync(id, tenantId, cancellationToken) ?? throw new NotFoundException("Not found");
    }
    public async Task<List<StockTransferBO>> GetAllTransfersAsync(CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        return await _dac.GetAllTransfersAsync(tenantId, cancellationToken);
    }
}
""")

with open(f"{CTRL_INV}/StockTransferController.cs", "w") as f:
    f.write("""using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.InventoryManagement.Requests;
using VGS.RetailOS.Modules.InventoryManagement.StockTransfer.IBL;
namespace VGS.RetailOS.ApiHost.Controllers.V1.InventoryManagement;

[ApiController]
[Route("api/v1/stock-transfer")]
[Authorize]
public class StockTransferController : ControllerBase
{
    private readonly IStockTransferBL _bl;
    public StockTransferController(IStockTransferBL bl) { _bl = bl; }

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiateStockTransferRequest request, CancellationToken cancellationToken)
    {
        var res = await _bl.InitiateTransferAsync(request, cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _bl.GetAllTransfersAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _bl.GetTransferAsync(id, cancellationToken));
    }
}
""")

# ----------------- RETURNS & REFUNDS -----------------
with open(f"{MOD_RET}/BO/ReturnBO.cs", "w") as f:
    f.write("""namespace VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
public class ReturnBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string ReturnNumber { get; set; } = null!;
    public string ReturnType { get; set; } = null!;
    public Guid StoreId { get; set; }
    public decimal TotalAmount { get; set; }
}
""")

with open(f"{MOD_RET}/IDAC/IReturnDAC.cs", "w") as f:
    f.write("""using VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
namespace VGS.RetailOS.Modules.ReturnsManagement.Return.IDAC;
public interface IReturnDAC
{
    Task<ReturnBO> CreateReturnAsync(ReturnBO returnBo, CancellationToken cancellationToken);
    Task<List<ReturnBO>> GetAllReturnsAsync(string tenantId, CancellationToken cancellationToken);
}
""")

with open(f"{INF_RET}/ReturnDAC.cs", "w") as f:
    f.write("""using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.ReturnsManagement.DAC.Entities;
using VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
using VGS.RetailOS.Modules.ReturnsManagement.Return.IDAC;

namespace VGS.RetailOS.Infrastructure.ReturnsManagement.DAC;
public class ReturnDAC : IReturnDAC
{
    private readonly AppDbContext _dbContext;
    public ReturnDAC(AppDbContext dbContext) { _dbContext = dbContext; }

    public async Task<ReturnBO> CreateReturnAsync(ReturnBO returnBo, CancellationToken cancellationToken)
    {
        var entity = new ReturnEntity {
            Id = returnBo.Id, TenantId = returnBo.TenantId, ReturnNumber = returnBo.ReturnNumber,
            ReturnType = returnBo.ReturnType, StoreId = returnBo.StoreId, TotalAmount = returnBo.TotalAmount
        };
        _dbContext.Returns.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return returnBo;
    }

    public async Task<List<ReturnBO>> GetAllReturnsAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Returns.AsNoTracking().Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken);
        return entities.Select(e => new ReturnBO { Id = e.Id, TenantId = e.TenantId, ReturnNumber = e.ReturnNumber, ReturnType = e.ReturnType, StoreId = e.StoreId, TotalAmount = e.TotalAmount }).ToList();
    }
}
""")

# Create mock requests for returns
with open(f"{BASE}/Contracts/VGS.RetailOS.Contracts/V1/ReturnsManagement/Requests/CreateReturnRequest.cs", "w") as f:
    f.write("""namespace VGS.RetailOS.Contracts.V1.ReturnsManagement.Requests;
public class CreateReturnRequest
{
    public Guid StoreId { get; set; }
    public string ReturnType { get; set; } = "CustomerReturn";
    public decimal TotalAmount { get; set; }
}
""")

with open(f"{MOD_RET}/IBL/IReturnBL.cs", "w") as f:
    f.write("""using VGS.RetailOS.Contracts.V1.ReturnsManagement.Requests;
using VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
namespace VGS.RetailOS.Modules.ReturnsManagement.Return.IBL;
public interface IReturnBL
{
    Task<ReturnBO> ProcessReturnAsync(CreateReturnRequest request, CancellationToken cancellationToken);
    Task<List<ReturnBO>> GetAllReturnsAsync(CancellationToken cancellationToken);
}
""")

with open(f"{MOD_RET}/BL/ReturnBL.cs", "w") as f:
    f.write("""using VGS.RetailOS.Contracts.V1.ReturnsManagement.Requests;
using VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
using VGS.RetailOS.Modules.ReturnsManagement.Return.IBL;
using VGS.RetailOS.Modules.ReturnsManagement.Return.IDAC;
using VGS.RetailOS.Shared.Tenancy;
using VGS.RetailOS.Shared.Errors.Exceptions;
namespace VGS.RetailOS.Modules.ReturnsManagement.Return.BL;
public class ReturnBL : IReturnBL
{
    private readonly IReturnDAC _dac;
    private readonly ITenantContextAccessor _tenantAccessor;
    public ReturnBL(IReturnDAC dac, ITenantContextAccessor tenantAccessor) { _dac = dac; _tenantAccessor = tenantAccessor; }

    public async Task<ReturnBO> ProcessReturnAsync(CreateReturnRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        var bo = new ReturnBO {
            Id = Guid.NewGuid(), TenantId = tenantId, ReturnNumber = $"RET-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ReturnType = request.ReturnType, StoreId = request.StoreId, TotalAmount = request.TotalAmount
        };
        return await _dac.CreateReturnAsync(bo, cancellationToken);
    }
    public async Task<List<ReturnBO>> GetAllReturnsAsync(CancellationToken cancellationToken)
    {
        var tenantId = _tenantAccessor.TenantContext?.CurrentTenantId ?? throw new UnauthorizedException("No tenant");
        return await _dac.GetAllReturnsAsync(tenantId, cancellationToken);
    }
}
""")

with open(f"{CTRL_RET}/ReturnController.cs", "w") as f:
    f.write("""using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.ReturnsManagement.Requests;
using VGS.RetailOS.Modules.ReturnsManagement.Return.IBL;
namespace VGS.RetailOS.ApiHost.Controllers.V1.ReturnsManagement;

[ApiController]
[Route("api/v1/return")]
[Authorize]
public class ReturnController : ControllerBase
{
    private readonly IReturnBL _bl;
    public ReturnController(IReturnBL bl) { _bl = bl; }

    [HttpPost]
    public async Task<IActionResult> CreateReturn([FromBody] CreateReturnRequest request, CancellationToken cancellationToken)
    {
        var res = await _bl.ProcessReturnAsync(request, cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _bl.GetAllReturnsAsync(cancellationToken));
    }
}
""")

