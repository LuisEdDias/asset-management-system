namespace AssetManagement.Shared.Users.Dtos;

public sealed record UpdateUserRequest{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}