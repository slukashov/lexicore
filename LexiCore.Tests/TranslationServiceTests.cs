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
    var metadata = new List<Metadata>
    {
      new() { Key = "btn_save", VariablesJson = "{\"test\": 1}" }
    };

    var mockDbSet = data.BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);
    var mockMetaDbSet = metadata.BuildMockDbSet();
    _dbContextMock.KeyMetadatas.Returns(mockMetaDbSet);

    var result = await _sut.GetAllAsync();

    Assert.NotNull(result);
    Assert.Equal(2, result.Count);
    Assert.Equal("{\"test\": 1}", result[0].VariablesJson);
    Assert.Equal("{\"test\": 1}", result[1].VariablesJson);
  }

  [Fact]
  public async Task UpsertAsync_ShouldAddNewEntry_WhenItDoesNotExist()
  {
    var mockDbSet = new List<Translation>().BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);
    var mockMetaDbSet = new List<Metadata>().BuildMockDbSet();
    _dbContextMock.KeyMetadatas.Returns(mockMetaDbSet);

    var newEntry = new LexiCoreEntry { Key = "hello", Culture = "en-US", Value = "Hello", VariablesJson = "{}" };

    await _sut.UpsertAsync(newEntry);

    mockDbSet.Received(1).Add(Arg.Is<Translation>(t => t.Key == "hello"));
    mockMetaDbSet.Received(1).Add(Arg.Is<Metadata>(m => m.Key == "hello"));
    await _dbContextMock.Received(1).SaveChangesAsync();
    _cacheMock.Received(1).Remove("translations:en-US");
  }

  [Fact]
  public async Task UpsertAsync_ShouldResetIdToZero_WhenAddingNewEntry()
  {
    var mockDbSet = new List<Translation>().BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);
    var mockMetaDbSet = new List<Metadata>().BuildMockDbSet();
    _dbContextMock.KeyMetadatas.Returns(mockMetaDbSet);

    var newEntryWithId = new LexiCoreEntry { Id = 999, Key = "hello", Culture = "en-US", Value = "Hello" };

    await _sut.UpsertAsync(newEntryWithId);

    // In my new implementation, I create a new Translation object, so Id will be 0 by default.
    mockDbSet.Received(1).Add(Arg.Is<Translation>(t => t.Id == 0));
    await _dbContextMock.Received(1).SaveChangesAsync();
  }

  [Fact]
  public async Task UpsertAsync_ShouldUpdateExistingEntry_WhenItExists()
  {
    var existingEntry = new Translation { Key = "hello", Culture = "en-US", Value = "Old Value", IsDeprecated = false };
    var existingMeta = new Metadata { Key = "hello", VariablesJson = "{\"old\":1}" };
    
    var mockDbSet = new List<Translation> { existingEntry }.BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);
    var mockMetaDbSet = new List<Metadata> { existingMeta }.BuildMockDbSet();
    _dbContextMock.KeyMetadatas.Returns(mockMetaDbSet);

    var updatedEntry = new LexiCoreEntry { Key = "hello", Culture = "en-US", Value = "New Value", VariablesJson = "{\"new\":1}", IsDeprecated = true };

    await _sut.UpsertAsync(updatedEntry);

    Assert.Equal("New Value", existingEntry.Value);
    Assert.True(existingEntry.IsDeprecated);
    Assert.Equal("{\"new\":1}", existingMeta.VariablesJson);
    mockDbSet.DidNotReceiveWithAnyArgs().Add(Arg.Any<Translation>());
    await _dbContextMock.Received(1).SaveChangesAsync();
    _cacheMock.Received(1).Remove("translations:en-US");
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
    _cacheMock.Received(1).Remove("translations:en-US");
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