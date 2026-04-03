using System.Globalization;
using DotLiquid;
using LexiCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace LexiCore.Services.Implementations;

/// <inheritdoc/>
internal class TranslationStringLocalizer(IServiceScopeFactory factory) : IStringLocalizer
{
  /// <inheritdoc/>
  public LocalizedString this[string name] => GetString(name);
  
  /// <inheritdoc/>
  public LocalizedString this[string name, params object[] args] => GetString(name, args);

  /// <inheritdoc/>
  public IEnumerable<LocalizedString> GetAllStrings(bool inc) =>
    GetCachedDict(CultureInfo.CurrentUICulture.Name)
      .Select(pair => new LocalizedString(pair.Key, pair.Value, false))
      .ToList();

  /// <summary>
  /// Retrieves a localized string based on the given key and optional formatting arguments.
  /// </summary>
  /// <param name="name">The key of the string to be localized.</param>
  /// <param name="args">Optional arguments used for formatting the localized string.</param>
  /// <returns>
  /// A <see cref="LocalizedString"/> object containing the localized value if found, or
  /// the key itself if no translation is available.
  /// </returns>
  private LocalizedString GetString(string name, params object[]? args)
  {
    var culture = CultureInfo.CurrentUICulture.Name;
    var translations = GetCachedDict(culture);
    var isFound = translations.TryGetValue(name, out var val);

    if (!isFound)
      return new LocalizedString(name, name, true);

    if (args == null || args.Length == 0)
      return new LocalizedString(name, val, false);

    Hash hash;
    if (args is [not null] && args[0].GetType().IsClass && args[0] is not string)
    {
      hash = Hash.FromAnonymousObject(args[0]);
    }
    else
    {
      hash = new Hash();
    }

    for (int i = 0; i < args.Length; i++)
    {
      hash[$"arg{i}"] = args[i];
    }

    var result = Template.Parse(val).Render(hash);
    if (result != val || !val.Contains("{0}")) 
      return new LocalizedString(name, result, false);
    
    try
    {
      result = string.Format(val, args);
    }
    catch
    {
      // ignored
    }

    return new LocalizedString(name, result, false);
  }

  /// <summary>
  /// Retrieves a dictionary of translations for the specified culture, utilizing caching for improved performance.
  /// </summary>
  /// <param name="culture">The culture identifier for which translations are requested.</param>
  /// <returns>A dictionary containing translation keys and their corresponding values for the specified culture.</returns>
  private Dictionary<string, string> GetCachedDict(string culture)
  {
    using var scope = factory.CreateScope();
    var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
    var cached = memoryCache.Get<Dictionary<string, string>>($"translations:{culture}");
    if (cached != null)
      return cached;

    var context = scope.ServiceProvider.GetRequiredService<ITranslationDbContext>();

    var translations = context.Translations
      .AsNoTracking()
      .Where(t => t.Culture == culture && !t.IsDeprecated)
      .ToDictionary(t => t.Key, t => t.Value);

    memoryCache.Set($"translations:{culture}", translations);
    return translations;
  }
}