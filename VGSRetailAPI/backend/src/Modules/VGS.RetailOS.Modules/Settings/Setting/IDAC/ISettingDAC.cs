using VGS.RetailOS.Modules.Settings.Setting.BO;

namespace VGS.RetailOS.Modules.Settings.Setting.IDAC;

public interface ISettingDAC
{
    Task<SettingBO?> GetSettingAsync(string key, string tenantId, Guid? storeId, CancellationToken cancellationToken);
    Task<List<SettingBO>> GetAllSettingsAsync(string tenantId, Guid? storeId, CancellationToken cancellationToken);
    Task<SettingBO> UpsertSettingAsync(SettingBO setting, CancellationToken cancellationToken);
}
