using LexiCore.Models;

namespace LexiCore.Services;

internal interface ITranslationService
{
  Task<IReadOnlyList<Translation>> GetAllAsync();
  Task UpsertAsync(Translation entry);
  Task DeleteAsync(string key, string culture);
}