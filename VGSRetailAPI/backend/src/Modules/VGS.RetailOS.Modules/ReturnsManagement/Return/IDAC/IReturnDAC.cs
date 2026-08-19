using VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
namespace VGS.RetailOS.Modules.ReturnsManagement.Return.IDAC;
public interface IReturnDAC
{
    Task<ReturnBO> CreateReturnAsync(ReturnBO returnBo, CancellationToken cancellationToken);
    Task<List<ReturnBO>> GetAllReturnsAsync(string tenantId, CancellationToken cancellationToken);
}
