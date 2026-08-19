using System.ComponentModel.DataAnnotations;

namespace VGS.RetailOS.Contracts.V1.CustomerManagement.Requests;

public class UpdateCustomerRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Required]
    [MaxLength(20)]
    public string Mobile { get; set; } = null!;

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
