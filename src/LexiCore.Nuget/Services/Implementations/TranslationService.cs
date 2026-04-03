using LexiCore.Data;
using LexiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LexiCore.Services.Implementations;

internal class TranslationService(ITranslationDbContext context, IMemoryCache cache) : ITranslationService
{
  public async Task<IReadOnlyList<LexiCoreEntry>> GetAllAsync()
  {
    return await context.Translations
      .AsNoTracking()
      .GroupJoin(
        context.KeyMetadatas.AsNoTracking(),
        translation => translation.Key,
        metadata => metadata.Key,
        (translation, metadatas) => new { Translation = translation, Metadata = metadatas.FirstOrDefault() }
      )
      .Select(x => new LexiCoreEntry
      {
        Id = x.Translation.Id,
        Key = x.Translation.Key,
        Culture = x.Translation.Culture,
        Value = x.Translation.Value,
        IsDeprecated = x.Translation.IsDeprecated,
        VariablesJson = x.Metadata != null ? x.Metadata.VariablesJson : null
      })
      .ToListAsync();
  }

  public async Task UpsertAsync(LexiCoreEntry entry)
  {
    var existing = await context.Translations
      .SingleOrDefaultAsync(translation => translation.Key == entry.Key && translation.Culture == entry.Culture);

    if (existing != null)
    {
      existing.Value = entry.Value;
      existing.IsDeprecated = entry.IsDeprecated;
    }
    else
    {
      context.Translations.Add(new Translation
      {
        Key = entry.Key,
        Culture = entry.Culture,
        Value = entry.Value,
        IsDeprecated = entry.IsDeprecated
      });
    }

    var existingMetadata = await context.KeyMetadatas
      .SingleOrDefaultAsync(m => m.Key == entry.Key);

    if (existingMetadata != null)
    {
      existingMetadata.VariablesJson = entry.VariablesJson;
    }
    else
    {
      context.KeyMetadatas.Add(new Metadata
      {
        Key = entry.Key,
        VariablesJson = entry.VariablesJson
      });
    }

    await context.SaveChangesAsync();
    cache.Remove($"translations:{entry.Culture}");
  }
  
  public async Task DeleteAsync(string key, string culture)
  {
    var entry = await context.Translations
      .SingleOrDefaultAsync(translation => translation.Key == key && translation.Culture == culture);

    if (entry != null)
    {
      context.Translations.Remove(entry);
      await context.SaveChangesAsync();
      cache.Remove($"translations:{culture}");
    }
  }
}