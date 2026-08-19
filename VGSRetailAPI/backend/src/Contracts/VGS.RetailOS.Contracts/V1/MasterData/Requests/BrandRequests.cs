namespace VGS.RetailOS.Contracts.V1.MasterData.Requests;

public class CreateBrandRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateBrandRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
