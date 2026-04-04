using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace LexiCore.Services.Implementations;

/// <inheritdoc/>
internal class TranslationStringLocalizerFactory(IServiceScopeFactory factory) : IStringLocalizerFactory
{
  /// <inheritdoc/>
  public IStringLocalizer Create(Type type) => new TranslationStringLocalizer(factory);

  /// <inheritdoc/>
  public IStringLocalizer Create(string baseName, string location) => new TranslationStringLocalizer(factory);
}