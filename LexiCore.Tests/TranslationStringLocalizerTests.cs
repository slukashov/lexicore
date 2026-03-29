using System.Globalization;
using LexiCore.Data;
using LexiCore.Models;
using LexiCore.Services.Implementations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace LexiCore.Tests;

public class TranslationStringLocalizerTests : IDisposable
{
  private readonly IServiceScopeFactory _scopeFactoryMock;
  private readonly ITranslationDbContext _dbContextMock;
  private readonly IMemoryCache _memoryCache;
  private readonly TranslationStringLocalizer _sut;
  private readonly CultureInfo _originalCulture;
  private readonly IServiceProvider _serviceProviderMock;

  public TranslationStringLocalizerTests()
  {
    _originalCulture = CultureInfo.CurrentUICulture;
    CultureInfo.CurrentUICulture = new CultureInfo("en-US");
    _memoryCache = new MemoryCache(new MemoryCacheOptions());
    _dbContextMock = Substitute.For<ITranslationDbContext>();
    _serviceProviderMock = Substitute.For<IServiceProvider>();
    _serviceProviderMock.GetService(typeof(IMemoryCache)).Returns(_memoryCache);
    _serviceProviderMock.GetService(typeof(ITranslationDbContext)).Returns(_dbContextMock);
    var scopeMock = Substitute.For<IServiceScope>();
    scopeMock.ServiceProvider.Returns(_serviceProviderMock);

    _scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
    _scopeFactoryMock.CreateScope().Returns(scopeMock);
    _sut = new TranslationStringLocalizer(_scopeFactoryMock);
  }

  private void SetupDatabase(List<Translation> data)
  {
    var mockDbSet = data.BuildMockDbSet();
    _dbContextMock.Translations.Returns(mockDbSet);
  }

  [Fact]
  public void Indexer_NameOnly_ShouldReturnTranslation_WhenExists()
  {
    SetupDatabase([new() { Key = "hello", Culture = "en-US", Value = "Hello World", IsDeprecated = false }]);

    var result = _sut["hello"];

    Assert.False(result.ResourceNotFound);
    Assert.Equal("hello", result.Name);
    Assert.Equal("Hello World", result.Value);
  }

  [Fact]
  public void Indexer_NameOnly_ShouldReturnNameAsValue_WhenNotExists()
  {
    SetupDatabase([]);

    var result = _sut["missing_key"];

    Assert.True(result.ResourceNotFound);
    Assert.Equal("missing_key", result.Name);
    Assert.Equal("missing_key", result.Value);
  }

  [Fact]
  public void Indexer_WithArgs_ShouldFormatString_WhenExists()
  {
    SetupDatabase([new() { Key = "welcome_user", Culture = "en-US", Value = "Welcome, {0}!", IsDeprecated = false }]);

    var result = _sut["welcome_user", "Alex"];

    Assert.False(result.ResourceNotFound);
    Assert.Equal("Welcome, Alex!", result.Value);
  }

  [Fact]
  public void GetAllStrings_ShouldReturnAllActiveTranslationsForCurrentCulture()
  {
    SetupDatabase([
      new() { Key = "one", Culture = "en-US", Value = "One", IsDeprecated = false },
      new() { Key = "two", Culture = "en-US", Value = "Two", IsDeprecated = true }, // Deprecated, should be ignored
      new() { Key = "three", Culture = "es-ES", Value = "Tres", IsDeprecated = false }
    ]);

    var results = _sut.GetAllStrings(false).ToList();

    Assert.Single(results);
    Assert.Equal("one", results[0].Name);
    Assert.Equal("One", results[0].Value);
  }

  [Fact]
  public void GetCachedDict_ShouldOnlyHitDatabaseOnce()
  {
    SetupDatabase([new() { Key = "test", Culture = "en-US", Value = "Test", IsDeprecated = false }]);

    _ = _sut["test"];
    _ = _sut["test"];
    var result = _sut["test"];

    _scopeFactoryMock.Received(3).CreateScope();
    _serviceProviderMock.Received(1).GetService(typeof(ITranslationDbContext));
    Assert.Equal("Test", result.Value);
  }

  public void Dispose()
  {
    CultureInfo.CurrentUICulture = _originalCulture;
    _memoryCache.Dispose();
  }
}