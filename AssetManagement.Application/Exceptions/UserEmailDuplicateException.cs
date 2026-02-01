namespace AssetManagement.Application.Exceptions;

public sealed class UserDuplicateEmailException : AppException
{
    public string Email { get; }

    public UserDuplicateEmailException(string email)
        : base("User.DuplicateEmail")
    {
        Email = email;
    }
}