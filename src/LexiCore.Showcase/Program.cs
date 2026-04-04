using LexiCore.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLexiCore(options =>
{
  // Use SQLite for the demo
  options.ConfigureDbContext = db => db.UseSqlite("Data Source=LexiCore_demo.db");

  // Define the languages your app supports
  options.SupportedCultures =
  [
    new("en-US"),
    new("es-ES"),
    new("fr-FR")
  ];
  
  options.RequireAuthorization = false; // Disable auth for the demo, but consider enabling it in production
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 2. Initialize the Database
await app.InitializeLexiCoreDatabaseAsync();

// 3. Map the Admin API and UI
app.MapLexiCoreApi();
app.UseLexiCoreUi(); // Accessible at http://localhost:PORT/lexi-core-ui

app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();