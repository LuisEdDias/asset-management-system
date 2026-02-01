using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.Assets.Dtos;
using AssetManagement.Application.Exceptions;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Assets;

public sealed class AssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetTypeRepository _typeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAssetAllocationLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssetService(
        IAssetRepository assets,
        IAssetTypeRepository types,
        IUserRepository users,
        IAssetAllocationLogRepository logs,
        IUnitOfWork uow)
    {
        _assetRepository = assets;
        _typeRepository = types;
        _userRepository = users;
        _logRepository = logs;
        _unitOfWork = uow;
    }

    public async Task<List<AssetResponse>> GetAllAsync(CancellationToken ct)
    {
        var items = await _assetRepository.GetAllAsyncNoTracking(ct);

        return items.Select(ToResponse).ToList();
    }

    public async Task<List<AssetResponse>> GetAllByAllocatedUserIdAsync(long userId, CancellationToken ct)
    {
        var items = await _assetRepository.GetAllByAllocatedUserIdAsync(userId, ct);

        return items.Select(ToResponse).ToList();
    }

    public async Task<AssetResponse> GetByIdAsync(long id, CancellationToken ct)
    {
        var asset = await _assetRepository.GetByIdAsync(id, ct) ?? throw new AssetNotFoundException(id);

        return ToResponse(asset);
    }

    public async Task<AssetResponse> CreateAsync(CreateAssetRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _typeRepository.ExistsAsync(request.AssetTypeId, ct))
            throw new AssetTypeNotFoundException(request.AssetTypeId);

        var normalizedSerial = NormalizeSerial(request.SerialNumber);

        if (await _assetRepository.ExistsBySerialAsync(normalizedSerial, ct))
            throw new AssetDuplicateSerialException(normalizedSerial);

        var entity = new Asset(
            request.Name,
            normalizedSerial,
            request.AssetTypeId,
            request.Value
        );

        _assetRepository.Add(entity);

        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(entity);
    }

    public async Task<AssetResponse> UpdateAsync(long id, UpdateAssetRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var asset = await _assetRepository.GetByIdAsync(id, ct) ?? throw new AssetNotFoundException(id);

        if (!await _typeRepository.ExistsAsync(request.AssetTypeId, ct))
            throw new AssetTypeNotFoundException(request.AssetTypeId);

        asset.UpdateName(request.Name);
        asset.ChangeType(request.AssetTypeId);
        asset.UpdateValue(request.Value);

        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(asset);
    }

    public async Task AllocateAsync(long assetId, long userId, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);

        var asset = await _assetRepository.GetByIdAsync(assetId, ct) ?? throw new AssetNotFoundException(assetId);

        if (asset.Status != AssetStatus.Available)
            throw new AssetNotAvailableException(assetId);

        if (!await _userRepository.ExistsAsync(userId, ct))
            throw new UserNotFoundException(userId);

        var nowUtc = DateTimeOffset.UtcNow;

        asset.AllocateTo(userId, nowUtc);

        _logRepository.Add(new AssetAllocationLog(asset.Id, userId, AllocationAction.Allocated, nowUtc));

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ReturnAsync(long assetId, CancellationToken ct)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId, ct) ?? throw new AssetNotFoundException(assetId);

        if (asset.Status != AssetStatus.InUse || !asset.AssignedToUserId.HasValue)
            throw new AssetReturnInvalidException(assetId);

        var previousUserId = asset.AssignedToUserId;

        asset.Return();

        _logRepository.Add(new AssetAllocationLog(asset.Id, previousUserId.Value, AllocationAction.Returned, DateTimeOffset.UtcNow));

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<List<AssetAllocationLogResponse>> GetHistoryAsync(long? assetId, long? userId, CancellationToken ct)
    {
        var logs = await _logRepository.GetHistoryAsync(assetId, userId, ct);

        return logs.Select(l => new AssetAllocationLogResponse(
            l.Id,
            l.AssetId,
            l.Asset.Name,
            l.UserId,
            l.User.Name,
            l.Action.ToString(),
            l.AtUtc
        )).ToList();
    }

    private static AssetResponse ToResponse(Asset a) => new(
        a.Id,
        a.Name,
        a.SerialNumber,
        a.AssetTypeId,
        a.Type?.Name,
        a.Status.ToString(),
        a.Value,
        a.AssignedToUserId,
        a.AssignedAtUtc
    );

    private static string NormalizeSerial(string serial)
        => serial.Trim();
}