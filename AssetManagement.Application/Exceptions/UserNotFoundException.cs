namespace AssetManagement.Application.Exceptions;

public sealed class UserNotFoundException : AppException
{
    public long UserId { get; }

    public UserNotFoundException(long id)
        : base("User.NotFound")
    {
        UserId = id;
    }
}