namespace VGS.RetailOS.Contracts.V1.MasterData.Responses;

public class TaxResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public string Type { get; set; } = null!;
    public bool IsActive { get; set; }
}
