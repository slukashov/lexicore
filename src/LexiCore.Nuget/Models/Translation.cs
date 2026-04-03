namespace LexiCore.Models;

/// <summary>
/// Represents a translation record in the localization system.
/// This class is used to store and manage localized text for a specific key and culture combination.
/// </summary>
internal record Translation
{
  /// <summary>
  /// Gets the unique identifier for the translation entry.
  /// </summary>
  /// <remarks>
  /// This property uniquely identifies a Translation record within the database.
  /// It is assigned automatically upon creation of a new record and remains unchanged.
  /// </remarks>
  public int Id { get; init; }

  /// <summary>
  /// Gets the unique key associated with a translation entry.
  /// </summary>
  /// <remarks>
  /// This property serves as an identifier for the translation text and is intended to be
  /// unique within the context of a specific culture. It is used to retrieve and manage
  /// translations in the system.
  /// </remarks>
  public string Key { get; init; } = string.Empty;

  /// <summary>
  /// Gets the culture associated with the translation entry.
  /// </summary>
  /// <remarks>
  /// This property specifies the culture or locale (e.g., "en-US", "fr-FR")
  /// that the translation entry is applicable to. It is used to differentiate
  /// translations of the same key for different languages or regions.
  /// </remarks>
  public string Culture { get; init; } = string.Empty;

  /// <summary>
  /// Gets or sets the translated value associated with a specific key and culture.
  /// </summary>
  /// <remarks>
  /// This property holds the actual text representation for a given translation. It is used to provide
  /// localized content corresponding to the specified key and culture. Changes to this property should
  /// reflect updates to the underlying translation data.
  /// </remarks>
  public string Value { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets a value indicating whether the translation entry is considered deprecated.
  /// </summary>
  /// <remarks>
  /// A deprecated translation is one that should no longer be used in the application.
  /// This property is used to filter out such translations during runtime and prevent them
  /// from being served or processed by the system.
  /// </remarks>
  public bool IsDeprecated { get; set; }
}