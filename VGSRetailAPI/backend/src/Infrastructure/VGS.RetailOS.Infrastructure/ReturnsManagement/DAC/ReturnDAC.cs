using Microsoft.EntityFrameworkCore;
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
