namespace VGS.RetailOS.Contracts.V1.MasterData.Requests;

public class CreateCategoryRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
