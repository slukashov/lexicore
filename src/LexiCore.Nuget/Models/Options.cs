using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace LexiCore.Models;

public class Options
{
    public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; set; }
    public List<CultureInfo> SupportedCultures { get; set; } = [new("en-US")];
    
    public bool RequireAuthorization { get; set; } = false;
    public string? AuthorizationPolicy { get; set; }

    /// <summary>
    /// If true, missing translation keys will be automatically created in the database
    /// when they are requested via the IStringLocalizer.
    /// </summary>
    public bool AutoCreateMissingKeys { get; set; } = false;
}