using Microsoft.EntityFrameworkCore;
using VGS.RetailOS.Infrastructure.Data;
using VGS.RetailOS.Infrastructure.InventoryManagement.DAC.Entities;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.BO;
using VGS.RetailOS.Modules.InventoryManagement.Inventory.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;

namespace VGS.RetailOS.Infrastructure.InventoryManagement.DAC;

public class InventoryDAC : IInventoryDAC
{
    private readonly AppDbContext _dbContext;

    public InventoryDAC(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InventoryLedgerBO> RecordTransactionAsync(InventoryLedgerBO transaction, CancellationToken cancellationToken)
    {
        // We use an explicit transaction to ensure both Ledger and Balance are updated atomically
        // If a transaction is already active (e.g., from PurchaseBL), use it instead of starting a new one.
        var dbTransaction = _dbContext.Database.CurrentTransaction ?? await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var isLocalTransaction = _dbContext.Database.CurrentTransaction == null;

        try
        {
            // 1. Get or create the stock balance record
            var balanceEntity = await _dbContext.StockBalances
                .FirstOrDefaultAsync(b => b.TenantId == transaction.TenantId && b.StoreId == transaction.StoreId && b.ProductId == transaction.ProductId, cancellationToken);

            if (balanceEntity == null)
            {
                balanceEntity = new StockBalanceEntity
                {
                    Id = Guid.NewGuid(),
                    TenantId = transaction.TenantId,
                    StoreId = transaction.StoreId,
                    ProductId = transaction.ProductId,
                    Quantity = 0,
                    LastUpdated = DateTimeOffset.UtcNow
                };
                _dbContext.StockBalances.Add(balanceEntity);
            }

            // 2. Apply the quantity change
            balanceEntity.Quantity += transaction.ChangeQuantity;
            balanceEntity.LastUpdated = DateTimeOffset.UtcNow;

            // 3. Set the BalanceAfter on the transaction to reflect the exact state after this change
            transaction.BalanceAfter = balanceEntity.Quantity;

            // 4. Create the ledger entry
            var ledgerEntity = new InventoryLedgerEntity
            {
                Id = Guid.NewGuid(),
                TenantId = transaction.TenantId,
                StoreId = transaction.StoreId,
                ProductId = transaction.ProductId,
                ChangeQuantity = transaction.ChangeQuantity,
                BalanceAfter = transaction.BalanceAfter,
                TransactionType = transaction.TransactionType,
                ReferenceId = transaction.ReferenceId,
                Reason = transaction.Reason,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.InventoryLedger.Add(ledgerEntity);

            // 5. Save changes
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (isLocalTransaction)
            {
                await dbTransaction.CommitAsync(cancellationToken);
            }

            transaction.Id = ledgerEntity.Id;
            transaction.CreatedAt = ledgerEntity.CreatedAt;

            return transaction;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (isLocalTransaction)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
            }
            throw new ValidationException("A concurrency error occurred while updating the stock balance. Please try again.");
        }
        catch (Exception)
        {
            if (isLocalTransaction)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
            }
            throw;
        }
        finally
        {
            if (isLocalTransaction && dbTransaction != null)
            {
                await dbTransaction.DisposeAsync();
            }
        }
    }

    public async Task<StockBalanceBO?> GetStockBalanceAsync(string tenantId, Guid storeId, Guid productId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.StockBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.StoreId == storeId && b.ProductId == productId, cancellationToken);

        if (entity == null) return null;

        return new StockBalanceBO
        {
            StoreId = entity.StoreId,
            ProductId = entity.ProductId,
            Quantity = entity.Quantity,
            LastUpdated = entity.LastUpdated
        };
    }

    public async Task<List<StockBalanceBO>> GetAllStockBalancesAsync(string tenantId, Guid storeId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.StockBalances
            .AsNoTracking()
            .Where(b => b.TenantId == tenantId && b.StoreId == storeId)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new StockBalanceBO
        {
            StoreId = e.StoreId,
            ProductId = e.ProductId,
            Quantity = e.Quantity,
            LastUpdated = e.LastUpdated
        }).ToList();
    }

    public async Task<List<InventoryLedgerBO>> GetStockHistoryAsync(string tenantId, Guid storeId, Guid productId, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.InventoryLedger
            .AsNoTracking()
            .Where(l => l.TenantId == tenantId && l.StoreId == storeId && l.ProductId == productId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new InventoryLedgerBO
        {
            Id = e.Id,
            TenantId = e.TenantId,
            StoreId = e.StoreId,
            ProductId = e.ProductId,
            ChangeQuantity = e.ChangeQuantity,
            BalanceAfter = e.BalanceAfter,
            TransactionType = e.TransactionType,
            ReferenceId = e.ReferenceId,
            Reason = e.Reason,
            CreatedAt = e.CreatedAt
        }).ToList();
    }
}
