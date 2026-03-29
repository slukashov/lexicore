using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace LexiCore.Models;

public class Options
{
    public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; set; }
    public List<CultureInfo> SupportedCultures { get; set; } = [new("en-US")];
}