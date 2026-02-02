using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Application.Assets;
using AssetManagement.Application.Exceptions;
using AssetManagement.Application.Assets.Dtos;
using AssetManagement.Application.Abstractions.Persistence;
using Moq;
using FluentAssertions;

namespace AssetManagement.Tests.Assets;

public sealed class AssetServiceTests
{
    private readonly Mock<IAssetRepository> _assetRepo = new(MockBehavior.Strict);
    private readonly Mock<IAssetTypeRepository> _typeRepo = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _userRepo = new(MockBehavior.Strict);
    private readonly Mock<IAssetAllocationLogRepository> _logRepo = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);

    private AssetService CreateSut()
        => new(_assetRepo.Object, _typeRepo.Object, _userRepo.Object, _logRepo.Object, _uow.Object);

    [Fact]
    public async Task GetAllAsync_should_return_mapped_assets()
    {
        // arrange
        var ct = CancellationToken.None;

        var a1Serial = "ABC";
        var a1Name = "Mouse";
        var a1 = AssetFactory.NewAvailable(
            name: a1Name, 
            serial: a1Serial, 
            typeId: 1L, 
            value: 100m);

        var a2Serial = "DEF";
        var a2Name = "Notebook";
        var a2UserId = 99L;
        var a2 = AssetFactory.NewInUse(
            name: a2Name, 
            serial: a2Serial, 
            typeId: 2L, 
            value: 5000m, 
            assignedTo: a2UserId, 
            assignedAtUtc: DateTimeOffset.UtcNow.AddDays(-1));

        _assetRepo.Setup(r => r.GetAllAsyncNoTracking(ct))
                  .ReturnsAsync(new List<Asset> { a1, a2 });

        var sut = CreateSut();

        // act
        var result = await sut.GetAllAsync(ct);

        // assert
        result.Should().HaveCount(2);

        result[0].Name.Should().Be(a1Name);
        result[0].SerialNumber.Should().Be(a1Serial);
        result[0].Status.Should().Be(AssetStatus.Available.ToString());
        result[0].AssignedToUserId.Should().BeNull();
        result[0].AssignedAt.Should().BeNull();

        result[1].Name.Should().Be(a2Name);
        result[1].SerialNumber.Should().Be(a2Serial);
        result[1].Status.Should().Be(AssetStatus.InUse.ToString());
        result[1].AssignedToUserId.Should().Be(a2UserId);
        result[1].AssignedAt.Should().NotBeNull();

        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task GetAllByAllocatedUserIdAsync_should_return_assets_for_user()
    {
        // arrange
        var ct = CancellationToken.None;
        var userId = 42L;

        var a1 = AssetFactory.NewInUse(
            name: "Fone", 
            serial: "X", 
            typeId: 1L, 
            value: 80m, 
            assignedTo: userId, 
            assignedAtUtc: DateTimeOffset.UtcNow.AddHours(-2));

        var a2 = AssetFactory.NewInUse(
            name: "Teclado", 
            serial: "Y", 
            typeId: 2L, 
            value: 150m, 
            assignedTo: userId, 
            assignedAtUtc: DateTimeOffset.UtcNow.AddHours(-1));

        _assetRepo.Setup(r => r.GetAllByAllocatedUserIdAsync(userId, ct))
                  .ReturnsAsync(new List<Asset> { a1, a2 });

        var sut = CreateSut();

        // act
        var result = await sut.GetAllByAllocatedUserIdAsync(userId, ct);

        // assert
        result.Should().HaveCount(2);
        result.All(r => r.AssignedToUserId == userId).Should().BeTrue();

        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_when_asset_exists_should_return_response()
    {
        // arrange
        var ct = CancellationToken.None;
        var name = "Monitor";
        var serial = "SER";
        var typeId = 5L;
        var value = 900m;

        var asset = AssetFactory.NewAvailable(name: name, serial: serial, typeId: typeId, value: value);

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync(asset);

        var sut = CreateSut();

        // act
        var result = await sut.GetByIdAsync(1, ct);

        // assert
        result.Name.Should().Be(name);
        result.SerialNumber.Should().Be(serial);
        result.AssetTypeId.Should().Be(typeId);
        result.Value.Should().Be(value);
        result.Status.Should().Be(AssetStatus.Available.ToString());

        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_when_asset_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;

        _assetRepo.Setup(r => r.GetByIdAsync(123, ct)).ReturnsAsync((Asset?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.GetByIdAsync(123, ct);

        // assert
        await act.Should().ThrowAsync<AssetNotFoundException>();

        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_when_request_null_should_throw()
    {
        // arrange
        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.CreateAsync(null!, CancellationToken.None);

        // assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_when_type_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var req = new CreateAssetRequest("Mouse", "  ABC  ", 1L, 100m);

        _typeRepo.Setup(r => r.ExistsAsync(1L, ct)).ReturnsAsync(false);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.CreateAsync(req, ct);

        // assert
        await act.Should().ThrowAsync<AssetTypeNotFoundException>();

        _typeRepo.VerifyAll();
        _assetRepo.Verify(r => r.Add(It.IsAny<Asset>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_should_normalize_serial_check_duplicate_add_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var name = "Mouse";
        var serial = "  ABC  ";
        var typeId = 1L;
        var value = 100m;
        var req = new CreateAssetRequest(name, serial, typeId, value);

        _typeRepo.Setup(r => r.ExistsAsync(typeId, ct)).ReturnsAsync(true);

        _assetRepo.Setup(r => r.ExistsBySerialAsync("ABC", ct)).ReturnsAsync(false);

        Asset? added = null;
        _assetRepo.Setup(r => r.Add(It.IsAny<Asset>()))
                  .Callback<Asset>(a => added = a);

        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        var result = await sut.CreateAsync(req, ct);

        // assert
        added.Should().NotBeNull();
        added.Name.Should().Be(name);
        added.SerialNumber.Should().Be("ABC");
        added.AssetTypeId.Should().Be(typeId);
        added.Value.Should().Be(value);
        result.Name.Should().Be(name);
        result.SerialNumber.Should().Be("ABC");
        result.AssetTypeId.Should().Be(typeId);
        result.Value.Should().Be(value);

        _assetRepo.Verify(r => r.Add(It.IsAny<Asset>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);
        _typeRepo.VerifyAll();
        _assetRepo.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_when_duplicate_serial_should_throw_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var req = new CreateAssetRequest("Mouse", " ABC ", 1L, 100m);

        _typeRepo.Setup(r => r.ExistsAsync(1L, ct)).ReturnsAsync(true);
        _assetRepo.Setup(r => r.ExistsBySerialAsync("ABC", ct)).ReturnsAsync(true);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.CreateAsync(req, ct);

        // assert
        await act.Should().ThrowAsync<AssetDuplicateSerialException>();

        _assetRepo.Verify(r => r.Add(It.IsAny<Asset>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _typeRepo.VerifyAll();
        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_when_request_null_should_throw()
    {
        var sut = CreateSut();
        Func<Task> act = () => sut.UpdateAsync(1, null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_when_asset_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var req = new UpdateAssetRequest("New", 1L, 999m);

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync((Asset?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.UpdateAsync(1, req, ct);

        // assert
        await act.Should().ThrowAsync<AssetNotFoundException>();

        _assetRepo.VerifyAll();
        _typeRepo.Verify(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_when_type_missing_should_throw_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var newTypeId = 10L;
        var req = new UpdateAssetRequest("New", newTypeId, 999m);
        var existing = AssetFactory.NewAvailable(name: "Old", serial: "S", typeId: 1L, value: 10m);

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync(existing);
        _typeRepo.Setup(r => r.ExistsAsync(newTypeId, ct)).ReturnsAsync(false);
        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.UpdateAsync(1, req, ct);

        // assert
        await act.Should().ThrowAsync<AssetTypeNotFoundException>();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _assetRepo.VerifyAll();
        _typeRepo.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_should_update_fields_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var NewName = "NewName";
        var newTypeId = 10L;
        var newValue = 999m;
        var req = new UpdateAssetRequest(NewName, newTypeId, newValue);
        var existing = AssetFactory.NewAvailable(name: "Old", serial: "S", typeId: 1L, value: 10m);

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync(existing);
        _typeRepo.Setup(r => r.ExistsAsync(newTypeId, ct)).ReturnsAsync(true);
        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        var res = await sut.UpdateAsync(1, req, ct);

        // assert 
        existing.Name.Should().Be(NewName);
        existing.AssetTypeId.Should().Be(newTypeId);
        existing.Value.Should().Be(newValue);

        res.Name.Should().Be(NewName);
        res.AssetTypeId.Should().Be(newTypeId);
        res.Value.Should().Be(newValue);

        _assetRepo.VerifyAll();
        _typeRepo.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task AllocateAsync_when_userId_invalid_should_throw()
    {
        var sut = CreateSut();

        Func<Task> act1 = () => sut.AllocateAsync(assetId: 1, userId: 0, CancellationToken.None);
        Func<Task> act2 = () => sut.AllocateAsync(assetId: 1, userId: -1, CancellationToken.None);

        await act1.Should().ThrowAsync<ArgumentOutOfRangeException>();
        await act2.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task AllocateAsync_when_asset_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync((Asset?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.AllocateAsync(1, 10, ct);

        // assert
        await act.Should().ThrowAsync<AssetNotFoundException>();

        _assetRepo.VerifyAll();
        _userRepo.Verify(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        _logRepo.Verify(r => r.Add(It.IsAny<AssetAllocationLog>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AllocateAsync_when_asset_not_available_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var asset = AssetFactory.NewInUse(name: "Notebook", serial: "S", typeId: 1L, value: 1m, assignedTo: 99L, assignedAtUtc: DateTimeOffset.UtcNow);

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync(asset);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.AllocateAsync(1, 10, ct);

        // assert
        await act.Should().ThrowAsync<AssetNotAvailableException>();

        _assetRepo.VerifyAll();
        _userRepo.Verify(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        _logRepo.Verify(r => r.Add(It.IsAny<AssetAllocationLog>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AllocateAsync_when_user_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var asset = AssetFactory.NewAvailable(name: "Mouse", serial: "S", typeId: 1L, value: 1m);
        var userId = 10L;

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync(asset);
        _userRepo.Setup(r => r.ExistsAsync(userId, ct)).ReturnsAsync(false);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.AllocateAsync(1, userId, ct);

        // assert
        await act.Should().ThrowAsync<UserNotFoundException>();

        _assetRepo.VerifyAll();
        _userRepo.VerifyAll();
        _logRepo.Verify(r => r.Add(It.IsAny<AssetAllocationLog>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AllocateAsync_should_allocate_add_log_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var asset = AssetFactory.NewAvailable(name: "Mouse", serial: "S", typeId: 1L, value: 1m);
        var userId = 10L;

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync(asset);
        _userRepo.Setup(r => r.ExistsAsync(userId, ct)).ReturnsAsync(true);

        AssetAllocationLog? capturedLog = null;
        _logRepo.Setup(r => r.Add(It.IsAny<AssetAllocationLog>()))
                .Callback<AssetAllocationLog>(l => capturedLog = l);

        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        var before = DateTimeOffset.UtcNow;

        // act
        await sut.AllocateAsync(assetId: 1, userId: userId, ct);

        var after = DateTimeOffset.UtcNow;

        // assert
        asset.Status.Should().Be(AssetStatus.InUse);
        asset.AssignedToUserId.Should().Be(userId);
        asset.AssignedAtUtc.Should().NotBeNull();
        asset.AssignedAtUtc.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        capturedLog.Should().NotBeNull();
        capturedLog.UserId.Should().Be(userId);
        capturedLog.Action.Should().Be(AllocationAction.Allocated);
        capturedLog.AtUtc.Should().BeCloseTo(asset.AssignedAtUtc.Value, TimeSpan.FromSeconds(2));

        _assetRepo.VerifyAll();
        _userRepo.VerifyAll();
        _logRepo.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task ReturnAsync_when_asset_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync((Asset?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.ReturnAsync(1, ct);

        // assert
        await act.Should().ThrowAsync<AssetNotFoundException>();

        _assetRepo.VerifyAll();
        _logRepo.Verify(r => r.Add(It.IsAny<AssetAllocationLog>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnAsync_when_status_invalid_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var asset = AssetFactory.NewAvailable(name: "Mouse", serial: "S", typeId: 1L, value: 1m);

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync(asset);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.ReturnAsync(1, ct);

        // assert
        await act.Should().ThrowAsync<AssetReturnInvalidException>();

        _assetRepo.VerifyAll();
        _logRepo.Verify(r => r.Add(It.IsAny<AssetAllocationLog>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnAsync_should_return_asset_add_log_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var previousUserId = 10L;
        var asset = AssetFactory.NewInUse(
            name: "Mouse", 
            serial: "S", 
            typeId: 1L, 
            value: 1m, 
            assignedTo: previousUserId, 
            assignedAtUtc: DateTimeOffset.UtcNow.AddMinutes(-30));

        _assetRepo.Setup(r => r.GetByIdAsync(1, ct)).ReturnsAsync(asset);

        AssetAllocationLog? capturedLog = null;
        _logRepo.Setup(r => r.Add(It.IsAny<AssetAllocationLog>()))
                .Callback<AssetAllocationLog>(l => capturedLog = l);

        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        var before = DateTimeOffset.UtcNow;

        // act
        await sut.ReturnAsync(1, ct);

        var after = DateTimeOffset.UtcNow;

        // assert
        asset.Status.Should().Be(AssetStatus.Available);
        asset.AssignedToUserId.Should().BeNull();
        asset.AssignedAtUtc.Should().BeNull();

        capturedLog.Should().NotBeNull();
        capturedLog.UserId.Should().Be(previousUserId);
        capturedLog.Action.Should().Be(AllocationAction.Returned);
        capturedLog.AtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        _assetRepo.VerifyAll();
        _logRepo.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task MarkMaintenanceAsync_when_asset_missing_should_throw_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var assetId = 1L;

        _assetRepo.Setup(r => r.GetByIdAsync(assetId, ct)).ReturnsAsync((Asset?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.MarkMaintenanceAsync(assetId, ct);

        // assert
        await act.Should().ThrowAsync<AssetNotFoundException>();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task MarkMaintenanceAsync_when_asset_not_available_should_throw_validation_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var assetId = 1L;

        var asset = AssetFactory.NewInUse(
            name: "Notebook",
            serial: "S",
            typeId: 1L,
            value: 100m,
            assignedTo: 10L,
            assignedAtUtc: DateTimeOffset.UtcNow.AddMinutes(-5));

        _assetRepo.Setup(r => r.GetByIdAsync(assetId, ct)).ReturnsAsync(asset);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.MarkMaintenanceAsync(assetId, ct);

        // assert
        await act.Should().ThrowAsync<AssetMaintenanceValidationException>();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        asset.Status.Should().Be(AssetStatus.InUse);
        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task MarkMaintenanceAsync_when_available_should_set_maintenance_clear_assignment_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var assetId = 1L;

        var asset = AssetFactory.NewAvailable(
            name: "Mouse",
            serial: "S",
            typeId: 1L,
            value: 1m);

        _assetRepo.Setup(r => r.GetByIdAsync(assetId, ct)).ReturnsAsync(asset);

        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        await sut.MarkMaintenanceAsync(assetId, ct);

        // assert
        asset.Status.Should().Be(AssetStatus.Maintenance);
        asset.AssignedToUserId.Should().BeNull();
        asset.AssignedAtUtc.Should().BeNull();

        _assetRepo.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_when_asset_missing_should_throw_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var assetId = 1L;

        _assetRepo.Setup(r => r.GetByIdAsync(assetId, ct)).ReturnsAsync((Asset?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.CompleteMaintenanceAsync(assetId, ct);

        // assert
        await act.Should().ThrowAsync<AssetNotFoundException>();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_when_asset_not_in_maintenance_should_throw_validation_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var assetId = 1L;

        var asset = AssetFactory.NewAvailable(
            name: "Mouse",
            serial: "S",
            typeId: 1L,
            value: 1m);

        _assetRepo.Setup(r => r.GetByIdAsync(assetId, ct)).ReturnsAsync(asset);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.CompleteMaintenanceAsync(assetId, ct);

        // assert
        await act.Should().ThrowAsync<AssetMaintenanceReturnValidationException>();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        asset.Status.Should().Be(AssetStatus.Available);
        _assetRepo.VerifyAll();
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_when_in_maintenance_should_set_available_clear_assignment_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var assetId = 1L;

        var asset = AssetFactory.NewAvailable(
            name: "Mouse",
            serial: "S",
            typeId: 1L,
            value: 1m);

        asset.MarkMaintenance();
        asset.Status.Should().Be(AssetStatus.Maintenance);

        _assetRepo.Setup(r => r.GetByIdAsync(assetId, ct))
                  .ReturnsAsync(asset);

        _uow.Setup(u => u.SaveChangesAsync(ct))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        await sut.CompleteMaintenanceAsync(assetId, ct);

        // assert
        asset.Status.Should().Be(AssetStatus.Available);
        asset.AssignedToUserId.Should().BeNull();
        asset.AssignedAtUtc.Should().BeNull();

        _assetRepo.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task GetHistoryAsync_should_map_logs()
    {
        // arrange
    var ct = CancellationToken.None;
    var at = DateTimeOffset.UtcNow.AddDays(-2);

    var assetId = 1L;
    var assetName = "Mouse";
    var userId = 10L;
    var userName = "Ana";

    var asset = AssetFactory.NewInUse(assetName, "ABC", 1L, 100m, userId, at);
    var user  = UserFactory.New(userName, "mail@example.com");

    var log = new AssetAllocationLog(assetId, userId, AllocationAction.Allocated, at);

    PrivateSetter.Set(log, nameof(AssetAllocationLog.Asset), asset);
    PrivateSetter.Set(log, nameof(AssetAllocationLog.User), user);

    _logRepo.Setup(r => r.GetHistoryAsync(assetId, null, ct))
            .ReturnsAsync(new List<AssetAllocationLog> { log });

    var sut = CreateSut();

    // act
    var result = await sut.GetHistoryAsync(assetId: assetId, userId: null, ct);

    // assert
    result.Should().HaveCount(1);
    var r0 = result[0];

    r0.AssetId.Should().Be(assetId);
    r0.AssetName.Should().Be(assetName);
    r0.UserId.Should().Be(userId);
    r0.UserName.Should().Be(userName);
    r0.Action.Should().Be(AllocationAction.Allocated.ToString());
    r0.AllocatedAtUtc.Should().Be(at);

    _logRepo.VerifyAll();
    }

    private static class AssetFactory
    {
        public static Asset NewAvailable(string name, string serial, long typeId, decimal value)
        {
            return new Asset(
                name: name,
                serialNumber: serial,
                assetTypeId: typeId,
                value: value
            );
        }

        public static Asset NewInUse(string name, string serial, long typeId, decimal value, long assignedTo, DateTimeOffset assignedAtUtc)
        {
            Asset asset = new(
                name: name,
                serialNumber: serial,
                assetTypeId: typeId,
                value: value
            );

            asset.AllocateTo(assignedTo, assignedAtUtc);

            return asset;
        }
    }

    private static class UserFactory
    {
        public static User New(string name, string email)
        {
            return new User(name: name, email: email);
        }
    }

    private static class AssetAllocationLogFactory
    {
        public static AssetAllocationLog New(long assetId, long userId, AllocationAction action, DateTimeOffset atUtc)
        {
            return new AssetAllocationLog(
                assetId: assetId,
                userId: userId,
                action: action,
                atUtc: atUtc
            );
        }
    }
}