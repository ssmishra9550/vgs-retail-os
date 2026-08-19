namespace VGS.RetailOS.Contracts.V1.User.Requests;

public class CreateUserRequest
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
}
