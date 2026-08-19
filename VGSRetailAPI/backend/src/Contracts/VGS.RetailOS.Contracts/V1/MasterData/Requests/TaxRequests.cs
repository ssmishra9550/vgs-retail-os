namespace VGS.RetailOS.Contracts.V1.MasterData.Requests;

public class CreateTaxRequest
{
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public string Type { get; set; } = null!; // Percentage or FixedAmount
}

public class UpdateTaxRequest
{
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public string Type { get; set; } = null!;
    public bool IsActive { get; set; }
}
