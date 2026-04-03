namespace LexiCore.Models;

/// <summary>
/// Represents a localization or translation entry within the LexiCore system.
/// Each instance of <see cref="LexiCoreEntry"/> encapsulates the key, culture, value, and associated metadata
/// for a specific translation record.
/// </summary>
/// <remarks>
/// This record is primarily used to store, identify, and retrieve localization information in a multi-language
/// application. The key serves as the identifier for the translation, and the culture defines the
/// regional or linguistic context. The optional metadata can provide additional details to support
/// complex translation scenarios.
/// </remarks>
internal record LexiCoreEntry
{
  /// <summary>
  /// Gets the unique identifier for the <see cref="LexiCoreEntry"/> record.
  /// </summary>
  /// <remarks>
  /// Used to uniquely identify a translation entry within the system. This property
  /// corresponds to the primary key in the underlying data storage.
  /// </remarks>
  public int Id { get; init; }

  /// <summary>
  /// Gets the unique key associated with the <see cref="LexiCoreEntry"/> record.
  /// </summary>
  /// <remarks>
  /// Represents the identifier used to associate a translation with its metadata and cultural context.
  /// This property is essential for linking translations with their corresponding metadata and for
  /// supporting translations lookup operations.
  /// </remarks>
  public string Key { get; init; } = string.Empty;

  /// <summary>
  /// Gets the cultural identifier associated with the <see cref="LexiCoreEntry"/> record.
  /// </summary>
  /// <remarks>
  /// Represents the language and regional settings for a specific translation entry. This property
  /// is used to differentiate translations based on cultural context, such as "en-US" for English (United States)
  /// or "fr-FR" for French (France).
  /// </remarks>
  public string Culture { get; init; } = string.Empty;

  /// <summary>
  /// Gets the localized text associated with a specific translation entry.
  /// </summary>
  /// <remarks>
  /// Represents the actual translated or templated content for a given key and culture.
  /// This property may include placeholders for dynamic values, which can be rendered
  /// using template engines like DotLiquid.
  /// </remarks>
  public string Value { get; init; } = string.Empty;

  /// <summary>
  /// Gets the JSON representation of variables associated with the <see cref="LexiCoreEntry"/> record.
  /// </summary>
  /// <remarks>
  /// This property stores optional metadata in JSON format that can be used to provide
  /// additional context or parameters for a translation entry. It can be null if no variables are defined.
  /// </remarks>
  public string? VariablesJson { get; init; } = "{}";

  /// <summary>
  /// Indicates whether the <see cref="LexiCoreEntry"/> record is deprecated.
  /// </summary>
  /// <remarks>
  /// If set to <c>true</c>, the entry is considered obsolete and should no longer be actively used.
  /// This property is updated as part of the translation lifecycle to manage the relevance of entries
  /// within the system.
  /// </remarks>
  public bool IsDeprecated { get; init; }
}
