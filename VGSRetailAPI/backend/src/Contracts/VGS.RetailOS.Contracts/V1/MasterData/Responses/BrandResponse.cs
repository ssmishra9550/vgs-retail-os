namespace VGS.RetailOS.Contracts.V1.MasterData.Responses;

public class BrandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
