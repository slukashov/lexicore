using LexiCore.Data;
using LexiCore.Models;
using LexiCore.Services.Implementations;
using Microsoft.Extensions.Caching.Memory;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace LexiCore.Tests;

public class TranslationServiceTests
{
  private readonly ITranslationDbContext _dbContextMock;
  private readonly IMemoryCache _cacheMock;
  private readonly TranslationService _sut;

  public TranslationServiceTests()
  {
    _dbContextMock = Substitute.For<ITranslationDbContext>();
    _cacheMock = Substitute.For<IMemoryCache>();
    _sut = new TranslationService(_dbContextMock, _cacheMock);
  }

  [Fact]
  public async Task GetAllAsync_ShouldReturnAllEntries()
  {
    var data = new List<Translation>
    {
      new() { Key = "btn_save", Culture = "en-US", Value = "Save" },
      new() { Key = "btn_save", Culture = "es-ES", Value = "Guardar" }
    };

    var mockDbSet = data.BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);

    var result = await _sut.GetAllAsync();

    Assert.NotNull(result);
    Assert.Equal(2, result.Count);
  }

  [Fact]
  public async Task UpsertAsync_ShouldAddNewEntry_WhenItDoesNotExist()
  {
    var mockDbSet = new List<Translation>().BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);

    var newEntry = new Translation { Key = "hello", Culture = "en-US", Value = "Hello" };

    await _sut.UpsertAsync(newEntry);

    mockDbSet.Received(1).Add(newEntry);
    await _dbContextMock.Received(1).SaveChangesAsync();
    _cacheMock.Received(1).Remove("translations:hello:en-US");
  }

  [Fact]
  public async Task UpsertAsync_ShouldUpdateExistingEntry_WhenItExists()
  {
    var existingEntry = new Translation { Key = "hello", Culture = "en-US", Value = "Old Value", IsDeprecated = false };
    var mockDbSet = new List<Translation> { existingEntry }.BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);

    var updatedEntry = new Translation { Key = "hello", Culture = "en-US", Value = "New Value", IsDeprecated = true };

    await _sut.UpsertAsync(updatedEntry);

    Assert.Equal("New Value", existingEntry.Value);
    Assert.True(existingEntry.IsDeprecated);

    mockDbSet.DidNotReceiveWithAnyArgs().Add(Arg.Any<Translation>());
    await _dbContextMock.Received(1).SaveChangesAsync();
    _cacheMock.Received(1).Remove("translations:hello:en-US");
  }

  [Fact]
  public async Task DeleteAsync_ShouldRemoveEntry_WhenMatchIsFound()
  {
    var existingEntry = new Translation { Key = "hello", Culture = "en-US", Value = "Hello" };
    var mockDbSet = new List<Translation> { existingEntry }.BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);

    await _sut.DeleteAsync("hello", "en-US");

    mockDbSet.Received(1).Remove(existingEntry);
    await _dbContextMock.Received(1).SaveChangesAsync();
    _cacheMock.Received(1).Remove("translations:hello:en-US");
  }

  [Fact]
  public async Task DeleteAsync_ShouldDoNothing_WhenNoMatchIsFound()
  {
    var mockDbSet = new List<Translation>().BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);

    await _sut.DeleteAsync("missing_key", "en-US");

    mockDbSet.DidNotReceiveWithAnyArgs().Remove(Arg.Any<Translation>());
    await _dbContextMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
    _cacheMock.DidNotReceiveWithAnyArgs().Remove(Arg.Any<Translation>());
  }
}