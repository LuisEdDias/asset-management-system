namespace AssetManagement.Shared.Users.Dtos;

public sealed record UserResponse{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}