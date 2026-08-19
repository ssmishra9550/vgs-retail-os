namespace VGS.RetailOS.Shared.Tenancy;

public class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<ITenantContextHolder> _tenantContextCurrent = new();

    public ITenantContext? TenantContext
    {
        get
        {
            return _tenantContextCurrent.Value?.Context;
        }
        set
        {
            var holder = _tenantContextCurrent.Value;
            if (holder != null)
            {
                // Clear current context if value is null
                holder.Context = null;
            }

            if (value != null)
            {
                // Set the current context
                _tenantContextCurrent.Value = new TenantContextHolder { Context = value };
            }
        }
    }

    private interface ITenantContextHolder
    {
        ITenantContext? Context { get; set; }
    }

    private class TenantContextHolder : ITenantContextHolder
    {
        public ITenantContext? Context { get; set; }
    }
}
