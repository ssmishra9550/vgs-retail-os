using VGS.RetailOS.Contracts.V1.Settings.Requests;
using VGS.RetailOS.Contracts.V1.Settings.Responses;

namespace VGS.RetailOS.Modules.Settings.Setting.IBL;

public interface ISettingBL
{
    Task<SettingResponse?> GetSettingAsync(string key, Guid? storeId, CancellationToken cancellationToken);
    Task<List<SettingResponse>> GetAllSettingsAsync(Guid? storeId, CancellationToken cancellationToken);
    Task<SettingResponse> UpsertSettingAsync(UpsertSettingRequest request, CancellationToken cancellationToken);
}
