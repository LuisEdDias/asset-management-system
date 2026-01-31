namespace AssetManagement.Application.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string messageKey) : base(messageKey)
    { 
        MessageKey = messageKey;
    }

    public string MessageKey { get;}
}