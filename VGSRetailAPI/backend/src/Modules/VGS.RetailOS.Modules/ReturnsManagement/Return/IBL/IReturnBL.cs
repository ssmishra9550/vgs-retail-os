using VGS.RetailOS.Contracts.V1.ReturnsManagement.Requests;
using VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
namespace VGS.RetailOS.Modules.ReturnsManagement.Return.IBL;
public interface IReturnBL
{
    Task<ReturnBO> ProcessReturnAsync(CreateReturnRequest request, CancellationToken cancellationToken);
    Task<List<ReturnBO>> GetAllReturnsAsync(CancellationToken cancellationToken);
}
