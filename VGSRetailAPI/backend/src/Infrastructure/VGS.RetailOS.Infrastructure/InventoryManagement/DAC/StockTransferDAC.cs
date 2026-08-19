using Microsoft.EntityFrameworkCore;
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
