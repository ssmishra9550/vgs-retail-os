using VGS.RetailOS.Shared.Audit;

namespace VGS.RetailOS.Infrastructure.MasterData.DAC.Entities;

public enum TaxType
{
    Percentage,
    FixedAmount
}

public class TaxEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public TaxType Type { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
