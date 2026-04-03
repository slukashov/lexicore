using LexiCore.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("AdminPolicy", policy =>
  {
    // Example: Require the user to be authenticated AND have an "Admin" role
    policy.RequireAuthenticatedUser();
    policy.RequireRole("Admin"); 
        
    // OR: Require a specific claim
    // policy.RequireClaim("Department", "IT");
  });
});
// 1. Register Lingo Services
builder.Services.AddLexiCore(options =>
{
  // Use SQLite for the demo
  options.ConfigureDbContext = db => db.UseSqlite("Data Source=LexiCore_demo.db");

  // Define the languages your app supports
  options.SupportedCultures =
  [
    new("en-US"),
    new("en-PL"),
    new("es-ES"),
    new("fr-FR")
  ];
  
  options.RequireAuthorization = false;
  options.AuthorizationPolicy = "AdminPolicy";
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 2. Initialize the Database
await app.InitializeLexiCoreDatabaseAsync();

// 3. Map the Admin API and UI
app.MapLexiCoreApi();      // Accessible at /api/lingo
app.UseLexiCoreUi(); // Accessible at http://localhost:PORT/lingo-admin

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();