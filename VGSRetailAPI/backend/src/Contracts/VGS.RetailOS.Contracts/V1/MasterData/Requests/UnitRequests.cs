namespace VGS.RetailOS.Contracts.V1.MasterData.Requests;

public class CreateUnitRequest
{
    public string Name { get; set; } = null!;
    public string ShortName { get; set; } = null!;
}

public class UpdateUnitRequest
{
    public string Name { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public bool IsActive { get; set; }
}
