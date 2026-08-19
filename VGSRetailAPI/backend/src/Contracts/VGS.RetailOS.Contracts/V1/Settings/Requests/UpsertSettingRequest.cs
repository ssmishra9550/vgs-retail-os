using System.ComponentModel.DataAnnotations;

namespace VGS.RetailOS.Contracts.V1.Settings.Requests;

public class UpsertSettingRequest
{
    public Guid? StoreId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Value { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Group { get; set; } = null!;
}
