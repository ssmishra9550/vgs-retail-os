namespace VGS.RetailOS.Shared.Errors.Exceptions;

/// <summary>
/// Exception thrown when a tenant context is required but cannot be resolved from the current request.
/// </summary>
public class TenantNotFoundException : BaseException
{
    public TenantNotFoundException(string message = "Tenant context could not be resolved for the current request.")
        : base(message, "TENANT_NOT_FOUND")
    {
    }
}
