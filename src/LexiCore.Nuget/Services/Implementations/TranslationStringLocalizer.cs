using System.Globalization;
using LexiCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace LexiCore.Services.Implementations;

internal class TranslationStringLocalizer(IServiceScopeFactory factory) : IStringLocalizer
{
  public LocalizedString this[string name] => GetString(name);
  public LocalizedString this[string name, params object[] args] => GetString(name, args);

  public IEnumerable<LocalizedString> GetAllStrings(bool inc) =>
    GetCachedDict(CultureInfo.CurrentUICulture.Name)
      .Select(pair => new LocalizedString(pair.Key, pair.Value, false))
      .ToList();

  private LocalizedString GetString(string name, params object[] args)
  {
    var dict = GetCachedDict(CultureInfo.CurrentUICulture.Name);
    var notFound = !dict.TryGetValue(name, out var val);
    val ??= name;
    if (!notFound && args.Length > 0)
      val = string.Format(val, args);

    return new LocalizedString(name, val, notFound);
  }

  private Dictionary<string, string> GetCachedDict(string culture)
  {
    using var scope = factory.CreateScope();
    var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
    return memoryCache.GetOrCreate($"translations:{culture}", entry =>
    {
      entry.SlidingExpiration = TimeSpan.FromHours(1);
      var context = scope.ServiceProvider.GetRequiredService<ITranslationDbContext>();
      return context.Translations
        .AsNoTracking()
        .Where(translation => translation.Culture == culture && !translation.IsDeprecated)
        .ToDictionary(translation => translation.Key, translation => translation.Value);
    }) ?? [];
  }
}