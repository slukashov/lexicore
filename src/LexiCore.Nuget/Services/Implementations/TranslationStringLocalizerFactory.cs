using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace LexiCore.Services.Implementations;

internal class TranslationStringLocalizerFactory(IServiceScopeFactory factory) : IStringLocalizerFactory
{
  public IStringLocalizer Create(Type type) => new TranslationStringLocalizer(factory);
  public IStringLocalizer Create(string baseName, string location) => new TranslationStringLocalizer(factory);
}