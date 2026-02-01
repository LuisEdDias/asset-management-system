using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Application.Exceptions;
using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.Users;
using AssetManagement.Application.Users.Dtos;
using Moq;
using FluentAssertions;

namespace AssetManagement.Tests.Users;

public sealed class UserServiceTests
{
    private readonly Mock<IUserRepository> _users = new(MockBehavior.Strict);
    private readonly Mock<IAssetAllocationLogRepository> _logRepo = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);

    private UserService CreateSut()
        => new(_users.Object, _logRepo.Object, _uow.Object);

    [Fact]
    public async Task GetAllAsync_should_return_mapped_users()
    {
        // arrange
        var ct = CancellationToken.None;

        var u1Name = "Ana";
        var u1Email = " ana@email.com ";
        var u1 = UserFactory.New(name: u1Name, email: u1Email);
        var u2Name = "Luis";
        var u2Email = "luis@mail.com";
        var u2 = UserFactory.New(name: u2Name, email: u2Email);

        _users.Setup(r => r.GetAllAsyncNoTracking(ct))
              .ReturnsAsync(new List<User> { u1, u2 });

        var sut = CreateSut();

        // act
        var result = await sut.GetAllAsync(ct);

        // assert
        result.Should().HaveCount(2);

        result[0].Name.Should().Be(u1Name);
        result[0].Email.Should().Be("ana@email.com");

        result[1].Name.Should().Be(u2Name);
        result[1].Email.Should().Be(u2Email);

        _users.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_when_user_exists_should_return_response()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 10L;

        var name = "Ana";
        var email = " ANA@Example.com ";
        var user = UserFactory.New(name: name, email: email);

        _users.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(user);

        var sut = CreateSut();

        // act
        var result = await sut.GetByIdAsync(id, ct);

        // assert
        result.Name.Should().Be(name);
        result.Email.Should().Be("ana@example.com");

        _users.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_when_user_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 999L;

        _users.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync((User?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.GetByIdAsync(id, ct);

        // assert
        await act.Should().ThrowAsync<UserNotFoundException>();

        _users.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_when_request_null_should_throw()
    {
        var sut = CreateSut();
        Func<Task> act = () => sut.CreateAsync(null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_when_email_already_exists_should_throw_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var req = new CreateUserRequest("Ana", "  ANA@Example.com  ");
        var normalizedEmail = "ana@example.com";

        _users.Setup(r => r.ExistsByEmailAsync(normalizedEmail, ct)).ReturnsAsync(true);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.CreateAsync(req, ct);

        // assert
        await act.Should().ThrowAsync<UserDuplicateEmailException>();

        _users.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _users.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_should_normalize_email_check_duplicate_add_and_save()
    {
        // arrange
        var ct = CancellationToken.None;

        var name = "Ana";
        var rawEmail = "  ANA@Example.com  ";
        var normalizedEmail = "ana@example.com";
        var req = new CreateUserRequest(name, rawEmail);

        _users.Setup(r => r.ExistsByEmailAsync(normalizedEmail, ct)).ReturnsAsync(false);

        User? added = null;
        _users.Setup(r => r.Add(It.IsAny<User>()))
              .Callback<User>(u => added = u);

        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        var result = await sut.CreateAsync(req, ct);

        // assert
        added.Should().NotBeNull();
        added.Name.Should().Be(name);
        added.Email.Should().Be(normalizedEmail);

        result.Name.Should().Be(name);
        result.Email.Should().Be(normalizedEmail);

        _users.Verify(r => r.ExistsByEmailAsync(normalizedEmail, ct), Times.Once);
        _users.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);

        _users.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_when_request_null_should_throw()
    {
        var sut = CreateSut();
        Func<Task> act = () => sut.UpdateAsync(1, null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_when_user_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;
        var req = new UpdateUserRequest("New", "new@example.com");

        _users.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync((User?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.UpdateAsync(id, req, ct);

        // assert
        await act.Should().ThrowAsync<UserNotFoundException>();

        _users.VerifyAll();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _users.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_when_email_not_changed_should_not_check_duplicate_and_should_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        var existing = UserFactory.New(name: "Ana", email: "ana@example.com");
        var req = new UpdateUserRequest("Ana Updated", "  ANA@Example.com  ");

        _users.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        var result = await sut.UpdateAsync(id, req, ct);

        // assert
        _users.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        existing.Name.Should().Be("Ana Updated");
        existing.Email.Should().Be("ana@example.com");

        result.Name.Should().Be("Ana Updated");
        result.Email.Should().Be("ana@example.com");

        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);

        _users.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_when_email_changed_and_duplicate_exists_should_throw_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        var existing = UserFactory.New(name: "Ana", email: "ana@example.com");
        var req = new UpdateUserRequest("Ana 2", "  ANA2@Example.com  ");
        var normalizedNewEmail = "ana2@example.com";

        _users.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(existing);
        _users.Setup(r => r.ExistsByEmailAsync(normalizedNewEmail, ct)).ReturnsAsync(true);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.UpdateAsync(id, req, ct);

        // assert
        await act.Should().ThrowAsync<UserDuplicateEmailException>();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _users.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_when_email_changed_and_not_duplicate_should_update_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        var existing = UserFactory.New(name: "Ana", email: "ana@example.com");

        var newName = "Ana Updated";
        var rawNewEmail = "  ANA2@Example.com  ";
        var normalizedNewEmail = "ana2@example.com";
        var req = new UpdateUserRequest(newName, rawNewEmail);

        _users.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(existing);
        _users.Setup(r => r.ExistsByEmailAsync(normalizedNewEmail, ct)).ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        var result = await sut.UpdateAsync(id, req, ct);

        // assert
        existing.Name.Should().Be(newName);
        existing.Email.Should().Be(normalizedNewEmail);

        result.Name.Should().Be(newName);
        result.Email.Should().Be(normalizedNewEmail);

        _users.Verify(r => r.ExistsByEmailAsync(normalizedNewEmail, ct), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);

        _users.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task GetAllocationLogAsync_should_map_logs()
    {
        // arrange
        var ct = CancellationToken.None;
        var userId = 10L;

        var at = DateTimeOffset.UtcNow.AddDays(-2);

        var assetName = "Mouse";
        var userName = "Ana";

        var asset = AssetFactory.NewAvailable(name: assetName, serial: "ABC", typeId: 1L, value: 100m);
        var user = UserFactory.New(name: userName, email: " ANA@Example.com ");

        PrivateSetter.Set(user, "Id", userId);

        var log = new AssetAllocationLog(assetId: 1L, userId: userId, action: AllocationAction.Allocated, atUtc: at);

        PrivateSetter.Set(log, nameof(AssetAllocationLog.Asset), asset);
        PrivateSetter.Set(log, nameof(AssetAllocationLog.User), user);

        _logRepo.Setup(r => r.GetHistoryAsync(null, userId, ct))
                .ReturnsAsync(new List<AssetAllocationLog> { log });

        var sut = CreateSut();

        // act
        var result = await sut.GetAllocationLogAsync(userId, ct);

        // assert
        result.Should().HaveCount(1);

        var r0 = result[0];
        r0.AssetName.Should().Be(assetName);
        r0.UserId.Should().Be(userId);
        r0.UserName.Should().Be(userName);
        r0.Action.Should().Be(AllocationAction.Allocated.ToString());
        r0.AllocatedAtUtc.Should().Be(at);

        _logRepo.VerifyAll();
    }

    private static class UserFactory
    {
        public static User New(string name, string email)
            => new User(name, email);
    }

    private static class AssetFactory
    {
        public static Asset NewAvailable(string name, string serial, long typeId, decimal value)
            => new Asset(name: name, serialNumber: serial, assetTypeId: typeId, value: value);
    }
}