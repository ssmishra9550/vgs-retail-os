namespace VGS.RetailOS.Modules.Settings.Setting.BO;

public class SettingBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public Guid? StoreId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string Group { get; set; } = null!;
}
