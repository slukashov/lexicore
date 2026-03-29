using LexiCore.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LexiCore.Tests;

public class TranslationStringLocalizerFactoryTests
{
  private readonly TranslationStringLocalizerFactory _sut;

  public TranslationStringLocalizerFactoryTests()
  {
    var scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
    _sut = new TranslationStringLocalizerFactory(scopeFactoryMock);
  }

  [Fact]
  public void Create_WithType_ShouldReturnLexiCoreStringLocalizer()
  {
    var result = _sut.Create(typeof(TranslationStringLocalizerFactoryTests));

    Assert.NotNull(result);
    Assert.IsType<TranslationStringLocalizer>(result);
  }

  [Fact]
  public void Create_WithBaseNameAndLocation_ShouldReturnLexiCoreStringLocalizer()
  {
    var result = _sut.Create("MyBaseName", "MyApp.Location");

    Assert.NotNull(result);
    Assert.IsType<TranslationStringLocalizer>(result);
  }
}