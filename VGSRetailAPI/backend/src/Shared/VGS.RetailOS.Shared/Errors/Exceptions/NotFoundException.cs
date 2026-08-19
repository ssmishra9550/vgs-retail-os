namespace VGS.RetailOS.Shared.Errors.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message) : base(message, "NOT_FOUND")
    {
    }
}
