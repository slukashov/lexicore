using LexiCore.Models;

namespace LexiCore.Services;

internal interface ITranslationService
{
  Task<IReadOnlyList<LexiCoreEntry>> GetAllAsync();
  Task UpsertAsync(LexiCoreEntry entry);
  Task DeleteAsync(string key, string culture);
}