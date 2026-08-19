namespace VGS.RetailOS.Contracts.V1.Settings.Responses;

public class SettingResponse
{
    public Guid Id { get; set; }
    public Guid? StoreId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string Group { get; set; } = null!;
}
