namespace VGS.RetailOS.Shared.Tenancy;

/// <summary>
/// Represents the tenant context for the current execution flow.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current Tenant ID.
    /// </summary>
    string CurrentTenantId { get; }

    /// <summary>
    /// Gets a value indicating whether the tenant has been successfully resolved for the current context.
    /// </summary>
    bool IsTenantResolved { get; }
}
