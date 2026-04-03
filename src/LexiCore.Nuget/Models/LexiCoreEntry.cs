namespace LexiCore.Models;

public record LexiCoreEntry
{
  public int Id { get; init; }
  public string Key { get; init; } = string.Empty;
  public string Culture { get; init; } = string.Empty;
  public string Value { get; init; } = string.Empty;
  public string? VariablesJson { get; init; } = "{}";
  public bool IsDeprecated { get; init; }
}
