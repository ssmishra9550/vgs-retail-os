namespace VGS.RetailOS.Contracts.V1.MasterData.Responses;

public class UnitResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public bool IsActive { get; set; }
}
