using System.ComponentModel.DataAnnotations;

namespace VGS.RetailOS.Contracts.V1.SupplierManagement.Requests;

public class UpdateSupplierRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [Required]
    [MaxLength(20)]
    public string Mobile { get; set; } = null!;

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? GstNumber { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
