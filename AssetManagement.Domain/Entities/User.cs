namespace AssetManagement.Domain.Entities;

public class User
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    public ICollection<Asset> AssignedAssets { get; private set; } = [];
    public ICollection<AssetAllocationLog> AllocationLogs { get; private set; } = [];

    private User() { }

    public User(string name, string email)
    {
        UpdateName(name);
        UpdateEmail(email);
    }

    public void UpdateName(string name)
    {
        Name = string.IsNullOrWhiteSpace(name)
        ? throw new ArgumentException("Name cannot be empty.")
        : name.Trim();
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.");

        var trimmedEmail = email.Trim().ToLowerInvariant();

        if (!trimmedEmail.Contains('@') || trimmedEmail.EndsWith("."))
            throw new ArgumentException("Invalid email format.", nameof(email));

        Email = trimmedEmail;
    }
}