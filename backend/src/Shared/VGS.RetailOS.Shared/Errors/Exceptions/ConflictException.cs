namespace VGS.RetailOS.Shared.Errors.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message) : base(message, "CONFLICT")
    {
    }
}
