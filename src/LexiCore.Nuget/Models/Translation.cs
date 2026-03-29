namespace LexiCore.Models;

internal record Translation
{
  public int Id { get; init; }
  public string Key { get; init; } = string.Empty;
  public string Culture { get; init; } = string.Empty;
  public string Value { get; set; } = string.Empty;
  public bool IsDeprecated { get; set; }
}