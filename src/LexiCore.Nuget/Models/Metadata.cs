namespace LexiCore.Models;

/// <summary>
/// Represents metadata associated with a specific key. This metadata provides
/// additional information about the key, such as related variables in JSON format.
/// </summary>
/// <remarks>
/// Metadata is used to store supplementary information for translation keys,
/// which can be helpful in scenarios where additional context or parameters
/// are required during runtime processing.
/// Key constraints:
/// - The combination of keys must be unique in the database.
/// - The <see cref="Key"/> property is immutable, ensuring the uniqueness constraint is upheld.
/// The <see cref="VariablesJson"/> property is flexible by allowing a JSON
/// string representation of various key-value pairs, which can be used for storing dynamic data.
/// </remarks>
internal record Metadata
{
  /// <summary>
  /// Represents a unique identifier for the metadata entry.
  /// </summary>
  /// <remarks>
  /// This property serves as a primary identifier used to distinguish metadata entries within the system.
  /// It is an integral component in managing and referencing metadata records programmatically.
  /// </remarks>
  public int Id { get; init; }

  /// <summary>
  /// Represents a unique identifier for translations and their associated metadata.
  /// </summary>
  /// <remarks>
  /// This property is used as a key to associate translations with their corresponding metadata entries.
  /// It is expected to be a unique string that serves as an identifier in localization operations.
  /// </remarks>
  public string Key { get; init; } = string.Empty;

  /// <summary>
  /// Represents a JSON-formatted string that stores metadata variables.
  /// </summary>
  /// <remarks>
  /// This property is used to store and retrieve metadata variables associated with a translation key.
  /// It is expected to hold a JSON string, providing flexibility for variable definitions.
  /// </remarks>
  public string? VariablesJson { get; set; } = "{}";
}
