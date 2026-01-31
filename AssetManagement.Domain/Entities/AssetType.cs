namespace AssetManagement.Domain.Entities;

public class AssetType
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;

    public ICollection<Asset> Assets { get; private set; } =[];

    private AssetType() { }

    public AssetType(string name)
    {
        Rename(name);
    }

    public void Rename(string name)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Name is required.")
            : name.Trim().ToUpperInvariant();
    }
}