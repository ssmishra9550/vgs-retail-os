namespace VGS.RetailOS.ApiHost.Contracts.V1.Auth;

public sealed record LoginRequest(string Email, string Password, string? TenantHint);
