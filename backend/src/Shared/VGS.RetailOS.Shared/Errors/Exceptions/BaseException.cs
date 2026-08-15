namespace VGS.RetailOS.Shared.Errors.Exceptions;

public abstract class BaseException : Exception
{
    public string ErrorCode { get; }

    protected BaseException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}
