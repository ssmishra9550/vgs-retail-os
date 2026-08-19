namespace VGS.RetailOS.Modules.MasterData.Tax.BO;

public class TaxBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public string Type { get; set; } = null!;
    public bool IsActive { get; set; }
}
