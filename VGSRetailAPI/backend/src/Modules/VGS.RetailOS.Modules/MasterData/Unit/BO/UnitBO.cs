namespace VGS.RetailOS.Modules.MasterData.Unit.BO;

public class UnitBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public bool IsActive { get; set; }
}
