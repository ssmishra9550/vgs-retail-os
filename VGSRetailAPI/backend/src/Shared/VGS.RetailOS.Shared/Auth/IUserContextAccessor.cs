namespace VGS.RetailOS.Shared.Auth;

public interface IUserContextAccessor
{
    Guid? CurrentUserId { get; }
}
