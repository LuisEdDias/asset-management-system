namespace AssetManagement.Application.Exceptions;

public sealed class AssetDuplicateSerialException : AppException
{
    public string SerialNumber { get; }
    public AssetDuplicateSerialException(string serial)
        : base("Asset.DuplicateSerial")
    {
        SerialNumber = serial;
    }
}