using AssetManagement.Domain.Entities;
using AssetManagement.Application.Exceptions;
using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.AssetTypes;
using AssetManagement.Application.AssetTypes.Dtos;
using Moq;
using FluentAssertions;

namespace AssetManagement.Tests.AssetTypes;

public sealed class AssetTypeServiceTests
{
    private readonly Mock<IAssetTypeRepository> _repo = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);

    private AssetTypeService CreateSut()
        => new(_repo.Object, _uow.Object);

    [Fact]
    public async Task GetAllAsync_should_return_mapped_types()
    {
        // arrange
        var ct = CancellationToken.None;

        var t1 = AssetTypeFactory.New(name: "Periférico");
        var t2 = AssetTypeFactory.New(name: "Notebook");

        _repo.Setup(r => r.GetAllAsyncNoTracking(ct))
             .ReturnsAsync(new List<AssetType> { t1, t2 });

        var sut = CreateSut();

        // act
        var result = await sut.GetAllAsync(ct);

        // assert
        result.Should().HaveCount(2);

        result[0].Name.Should().Be(t1.Name);
        result[1].Name.Should().Be(t2.Name);

        _repo.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_when_exists_should_return_response()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 10L;

        var type = AssetTypeFactory.New(name: "Periférico");
        PrivateSetter.Set(type, "Id", id);

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(type);

        var sut = CreateSut();

        // act
        var result = await sut.GetByIdAsync(id, ct);

        // assert
        result.Id.Should().Be(id);
        result.Name.Should().Be("PERIFÉRICO");

        _repo.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_when_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 999L;

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync((AssetType?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.GetByIdAsync(id, ct);

        // assert
        await act.Should().ThrowAsync<EntityNotFoundException>();

        _repo.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_when_request_null_should_throw()
    {
        var sut = CreateSut();
        Func<Task> act = () => sut.CreateAsync(null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_when_duplicate_name_should_throw_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var req = new CreateAssetTypeRequest("  periférico  ");
        var normalized = "PERIFÉRICO";

        _repo.Setup(r => r.ExistsByNameAsync(normalized, ct)).ReturnsAsync(true);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.CreateAsync(req, ct);

        // assert
        await act.Should().ThrowAsync<AssetTypeDuplicateNameException>();

        _repo.Verify(r => r.Add(It.IsAny<AssetType>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _repo.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_when_not_duplicate_should_add_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var req = new CreateAssetTypeRequest("  periférico  ");
        var normalized = "PERIFÉRICO";

        _repo.Setup(r => r.ExistsByNameAsync(normalized, ct)).ReturnsAsync(false);

        AssetType? added = null;
        _repo.Setup(r => r.Add(It.IsAny<AssetType>()))
             .Callback<AssetType>(e => added = e);

        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        var result = await sut.CreateAsync(req, ct);

        // assert
        added.Should().NotBeNull();
        added.Name.Should().Be(normalized);
        result.Name.Should().Be(added.Name);

        _repo.Verify(r => r.ExistsByNameAsync(normalized, ct), Times.Once);
        _repo.Verify(r => r.Add(It.IsAny<AssetType>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);

        _repo.VerifyAll();
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
    public async Task UpdateAsync_when_entity_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;
        var req = new UpdateAssetTypeRequest("New");

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync((AssetType?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.UpdateAsync(id, req, ct);

        // assert
        await act.Should().ThrowAsync<EntityNotFoundException>();

        _repo.VerifyAll();
        _repo.Verify(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_when_name_not_changed_should_not_check_duplicate_should_save_and_not_rename()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        var normalized = "PERIFÉRICO";
        var entity = AssetTypeFactory.New(name: "Periférico");
        var req = new UpdateAssetTypeRequest("  pErIFéRico  ");

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(entity);
        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        var result = await sut.UpdateAsync(id, req, ct);

        // assert
        _repo.Verify(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        entity.Name.Should().Be(normalized);
        result.Name.Should().Be(normalized);

        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);

        _repo.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_when_name_changed_and_duplicate_exists_should_throw_and_not_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        var entity = AssetTypeFactory.New(name: "Periférico");
        var req = new UpdateAssetTypeRequest("Notebook");
        var normalizedNew = "NOTEBOOK";

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(entity);
        _repo.Setup(r => r.ExistsByNameAsync(normalizedNew, ct)).ReturnsAsync(true);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.UpdateAsync(id, req, ct);

        // assert
        await act.Should().ThrowAsync<AssetTypeDuplicateNameException>();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _repo.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_when_name_changed_and_not_duplicate_should_rename_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        var entity = AssetTypeFactory.New(name: "Periférico");
        var req = new UpdateAssetTypeRequest("  Notebook  ");
        var normalizedNew = "NOTEBOOK";

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(entity);
        _repo.Setup(r => r.ExistsByNameAsync(normalizedNew, ct)).ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        var result = await sut.UpdateAsync(id, req, ct);

        // assert
        entity.Name.Should().Be(normalizedNew);
        result.Name.Should().Be(normalizedNew);

        _repo.Verify(r => r.ExistsByNameAsync(normalizedNew, ct), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);

        _repo.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task DeleteAsync_when_entity_missing_should_throw()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync((AssetType?)null);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.DeleteAsync(id, ct);

        // assert
        await act.Should().ThrowAsync<EntityNotFoundException>();

        _repo.VerifyAll();
        _repo.Verify(r => r.IsInUseAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.Remove(It.IsAny<AssetType>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_when_in_use_should_throw_and_not_remove()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        var entity = AssetTypeFactory.New(name: "Periférico");

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(entity);
        _repo.Setup(r => r.IsInUseAsync(id, ct)).ReturnsAsync(true);

        var sut = CreateSut();

        // act
        Func<Task> act = () => sut.DeleteAsync(id, ct);

        // assert
        await act.Should().ThrowAsync<AssetTypeInUseException>();

        _repo.Verify(r => r.Remove(It.IsAny<AssetType>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _repo.VerifyAll();
    }

    [Fact]
    public async Task DeleteAsync_when_not_in_use_should_remove_and_save()
    {
        // arrange
        var ct = CancellationToken.None;
        var id = 1L;

        var entity = AssetTypeFactory.New(name: "Periférico");

        _repo.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(entity);
        _repo.Setup(r => r.IsInUseAsync(id, ct)).ReturnsAsync(false);

        AssetType? removed = null;
        _repo.Setup(r => r.Remove(It.IsAny<AssetType>()))
             .Callback<AssetType>(e => removed = e);

        _uow.Setup(u => u.SaveChangesAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // act
        await sut.DeleteAsync(id, ct);

        // assert
        removed.Should().NotBeNull();
        removed.Name.Should().Be(entity.Name);

        _repo.Verify(r => r.Remove(It.IsAny<AssetType>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);

        _repo.VerifyAll();
        _uow.VerifyAll();
    }

    private static class AssetTypeFactory
    {
        public static AssetType New(string name)
            => new AssetType(name);
    }
}