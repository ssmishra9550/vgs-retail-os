using VGS.RetailOS.Modules.MasterData.Category.BO;

namespace VGS.RetailOS.Modules.MasterData.Category.IDAC;

public interface ICategoryDAC
{
    Task<CategoryBO?> GetCategoryByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken);
    Task<CategoryBO?> GetCategoryByNameAsync(string name, string tenantId, CancellationToken cancellationToken);
    Task<List<CategoryBO>> GetAllCategoriesAsync(string tenantId, CancellationToken cancellationToken);
    Task<CategoryBO> CreateCategoryAsync(CategoryBO category, CancellationToken cancellationToken);
    Task<CategoryBO> UpdateCategoryAsync(CategoryBO category, CancellationToken cancellationToken);
}
