namespace VGS.RetailOS.Shared.Errors.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message) 
        : base(message, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string message, string errorCode) 
        : base(message, errorCode)
    {
    }
}
