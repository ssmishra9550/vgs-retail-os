using VGS.RetailOS.Contracts.V1.Settings.Requests;
using VGS.RetailOS.Contracts.V1.Settings.Responses;
using VGS.RetailOS.Modules.Settings.Setting.BO;
using VGS.RetailOS.Modules.Settings.Setting.IBL;
using VGS.RetailOS.Modules.Settings.Setting.IDAC;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Tenancy;

namespace VGS.RetailOS.Modules.Settings.Setting.BL;

public class SettingBL : ISettingBL
{
    private readonly ISettingDAC _settingDac;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public SettingBL(ISettingDAC settingDac, ITenantContextAccessor tenantContextAccessor)
    {
        _settingDac = settingDac;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<SettingResponse?> GetSettingAsync(string key, Guid? storeId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        // 1. Try to fetch store-specific setting if storeId is provided
        if (storeId.HasValue)
        {
            var storeSetting = await _settingDac.GetSettingAsync(key, tenantId, storeId.Value, cancellationToken);
            if (storeSetting != null)
            {
                return MapToResponse(storeSetting);
            }
        }

        // 2. Fall back to global tenant setting
        var globalSetting = await _settingDac.GetSettingAsync(key, tenantId, null, cancellationToken);
        return globalSetting == null ? null : MapToResponse(globalSetting);
    }

    public async Task<List<SettingResponse>> GetAllSettingsAsync(Guid? storeId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var allSettings = await _settingDac.GetAllSettingsAsync(tenantId, storeId, cancellationToken);

        // Group by key. If a store setting exists, it overrides the global one.
        var resolvedSettings = new Dictionary<string, SettingBO>();

        foreach (var setting in allSettings)
        {
            // If the key is not in dictionary yet, add it
            if (!resolvedSettings.ContainsKey(setting.Key))
            {
                resolvedSettings[setting.Key] = setting;
            }
            else
            {
                // If the setting already exists, we prefer the Store-specific one over the Tenant-global one.
                // Since StoreId could be null (global), if the current item has a StoreId, it overrides.
                if (setting.StoreId.HasValue)
                {
                    resolvedSettings[setting.Key] = setting;
                }
            }
        }

        return resolvedSettings.Values.Select(MapToResponse).ToList();
    }

    public async Task<SettingResponse> UpsertSettingAsync(UpsertSettingRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var settingBo = new SettingBO
        {
            TenantId = tenantId,
            StoreId = request.StoreId,
            Key = request.Key,
            Value = request.Value,
            Group = request.Group
        };

        var result = await _settingDac.UpsertSettingAsync(settingBo, cancellationToken);
        return MapToResponse(result);
    }

    private string GetTenantId()
    {
        var tenantId = _tenantContextAccessor.TenantContext?.CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId))
            throw new UnauthorizedException("Tenant context is missing.");
        return tenantId;
    }

    private static SettingResponse MapToResponse(SettingBO bo)
    {
        return new SettingResponse
        {
            Id = bo.Id,
            StoreId = bo.StoreId,
            Key = bo.Key,
            Value = bo.Value,
            Group = bo.Group
        };
    }
}
