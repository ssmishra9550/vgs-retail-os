using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.ApiHost.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantContextAccessor)
    {
        // 1. Bypass tenant resolution for endpoints that do not require it
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        string? tenantId = null;

        // 2. Try to get Tenant ID from JWT claims (Primary method)
        if (context.User.Identity is { IsAuthenticated: true })
        {
            tenantId = context.User.FindFirst("tenant_id")?.Value;
        }

        // 3. Fallback to HTTP Header for S2S or specific scenarios
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValues))
            {
                tenantId = headerValues.FirstOrDefault();
            }
        }

        // 4. Validate and Set Context
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new TenantNotFoundException("Tenant ID is missing. Please provide a valid tenant context via JWT 'tenant_id' claim or 'X-Tenant-Id' header.");
        }

        // Set the context in AsyncLocal
        tenantContextAccessor.TenantContext = new TenantContext(tenantId);

        try
        {
            await _next(context);
        }
        finally
        {
            // Clean up to prevent context leaking
            tenantContextAccessor.TenantContext = null;
        }
    }
}
