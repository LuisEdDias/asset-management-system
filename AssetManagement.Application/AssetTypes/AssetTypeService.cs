using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Shared.AssetTypes.Dtos;
using AssetManagement.Application.Exceptions;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.AssetTypes;

public sealed class AssetTypeService
{
    private readonly IAssetTypeRepository _assetTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssetTypeService(IAssetTypeRepository assetTypeRepository, IUnitOfWork unitOfWork)
    {
        _assetTypeRepository = assetTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AssetTypeResponse>> GetAllAsync(CancellationToken ct)
    {
        var types = await _assetTypeRepository.GetAllAsyncNoTracking(ct);

        return types
            .Select(x => new AssetTypeResponse { Id = x.Id, Name = x.Name })
            .ToList();
    }

    public async Task<AssetTypeResponse> GetByIdAsync(long id, CancellationToken ct)
    {
        var type = await _assetTypeRepository.GetByIdAsync(id, ct) ?? throw new EntityNotFoundException(id);

        return new AssetTypeResponse { Id = type.Id, Name = type.Name };
    }

    public async Task<AssetTypeResponse> CreateAsync(CreateAssetTypeRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);


        var normalized = NormalizeName(request.Name);
        var exists = await _assetTypeRepository.ExistsByNameAsync(normalized, ct);
        if (exists)
            throw new AssetTypeDuplicateNameException(request.Name);

        var entity = new AssetType(request.Name);
        _assetTypeRepository.Add(entity);

        await _unitOfWork.SaveChangesAsync(ct);

        return new AssetTypeResponse { Id = entity.Id, Name = entity.Name };
    }

    public async Task<AssetTypeResponse> UpdateAsync(long id, UpdateAssetTypeRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _assetTypeRepository.GetByIdAsync(id, ct)
        ?? throw new EntityNotFoundException(id);

        var normalizedNewName = NormalizeName(request.Name);
        if (NormalizeName(entity.Name) != normalizedNewName)
        {
            if (await _assetTypeRepository.ExistsByNameAsync(normalizedNewName, ct))
                throw new AssetTypeDuplicateNameException(request.Name);

            entity.Rename(request.Name);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return new AssetTypeResponse { Id = entity.Id, Name = entity.Name };
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var entity = await _assetTypeRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(id);

        if (await _assetTypeRepository.IsInUseAsync(id, ct))
            throw new AssetTypeInUseException(entity.Name);

        _assetTypeRepository.Remove(entity);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static string NormalizeName(string name) => name.Trim().ToUpperInvariant();
}