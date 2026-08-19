namespace VGS.RetailOS.Shared.Tenancy;

/// <summary>
/// Provides access to the current <see cref="ITenantContext"/> across asynchronous boundaries.
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Gets or sets the current tenant context.
    /// </summary>
    ITenantContext? TenantContext { get; set; }
}
