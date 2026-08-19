using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;

namespace VGS.RetailOS.Modules.MasterData.Category.IBL;

public interface ICategoryBL
{
    Task<CategoryResponse> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<CategoryResponse>> GetAllCategoriesAsync(CancellationToken cancellationToken);
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<CategoryResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);
}
