namespace LexiCore.Models;

internal record Metadata
{
  public int Id { get; init; }
  public string Key { get; init; } = string.Empty;
  public string? VariablesJson { get; set; } = "{}";
}
