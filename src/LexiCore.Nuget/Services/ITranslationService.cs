using LexiCore.Models;

namespace LexiCore.Services;

/// <summary>
/// Defines methods for managing translation entries, including retrieving, inserting, updating,
/// and deleting translations.
/// </summary>
internal interface ITranslationService
{
  /// <summary>
  /// Retrieves all translation entries from the database, including optional metadata for supported keys.
  /// </summary>
  /// <returns>
  /// A task representing the asynchronous operation. The task result contains a read-only list of
  /// <see cref="LexiCoreEntry"/> objects that represent the translations.
  /// </returns>
  Task<IReadOnlyList<LexiCoreEntry>> GetAllAsync();

  /// <summary>
  /// Inserts a new translation entry or updates an existing one in the database.
  /// Removes the cached translation entry for the corresponding culture to ensure cache invalidation.
  /// </summary>
  /// <param name="entry">
  /// The <see cref="LexiCoreEntry"/> object representing the translation to be inserted or updated.
  /// </param>
  /// <returns>
  /// A task representing the asynchronous operation of inserting or updating a translation entry.
  /// </returns>
  Task UpsertAsync(LexiCoreEntry entry);

  /// <summary>
  /// Deletes a specific translation entry from the database based on the provided key and culture.
  /// </summary>
  /// <param name="key">The unique key that identifies the translation entry to be deleted.</param>
  /// <param name="culture">The culture associated with the translation entry to be deleted.</param>
  /// <returns>
  /// A task representing the asynchronous operation.
  /// </returns>
  Task DeleteAsync(string key, string culture);
}