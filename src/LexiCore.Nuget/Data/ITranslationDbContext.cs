using LexiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LexiCore.Data;

internal interface ITranslationDbContext
{
  DbSet<Translation> Translations { get; }
  DatabaseFacade Database { get; }

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}