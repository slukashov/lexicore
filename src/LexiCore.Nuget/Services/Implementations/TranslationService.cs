using LexiCore.Data;
using LexiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LexiCore.Services.Implementations;

internal class TranslationService(ITranslationDbContext context, IMemoryCache cache) : ITranslationService
{
  public async Task<IReadOnlyList<Translation>> GetAllAsync() =>
    await context.Translations.AsNoTracking().ToListAsync();

  public async Task UpsertAsync(Translation entry)
  {
    var existing = await context.Translations
      .SingleOrDefaultAsync(translation => translation.Key == entry.Key && translation.Culture == entry.Culture);

    if (existing != null)
    {
      existing.Value = entry.Value;
      existing.IsDeprecated = entry.IsDeprecated;
    }
    else context.Translations.Add(entry);

    await context.SaveChangesAsync();
    cache.Remove($"translations:{entry.Key}:{entry.Culture}");
  }

  public async Task DeleteAsync(string key, string culture)
  {
    var entry = await context.Translations
      .SingleOrDefaultAsync(translation => translation.Key == key && translation.Culture == culture);

    if (entry != null)
    {
      context.Translations.Remove(entry);
      await context.SaveChangesAsync();
      cache.Remove($"translations:{entry.Key}:{culture}");
    }
  }
}