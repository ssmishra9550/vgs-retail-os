namespace VGS.RetailOS.Shared.Tenancy;

public class TenantContext : ITenantContext
{
    public TenantContext(string currentTenantId)
    {
        if (string.IsNullOrWhiteSpace(currentTenantId))
        {
            throw new ArgumentException("Tenant ID cannot be null or empty.", nameof(currentTenantId));
        }

        CurrentTenantId = currentTenantId;
        IsTenantResolved = true;
    }

    public string CurrentTenantId { get; }
    public bool IsTenantResolved { get; }
}
