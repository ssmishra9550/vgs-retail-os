namespace VGS.RetailOS.Modules.MasterData.Category.BO;

public class CategoryBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
