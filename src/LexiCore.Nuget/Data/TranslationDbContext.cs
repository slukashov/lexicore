using LexiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace LexiCore.Data;

internal class TranslationDbContext(DbContextOptions<TranslationDbContext> options) : DbContext(options), ITranslationDbContext
{
  public DbSet<Translation> Translations { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder) =>
    modelBuilder.Entity<Translation>().HasIndex(entry => new { entry.Key, entry.Culture }).IsUnique();
}