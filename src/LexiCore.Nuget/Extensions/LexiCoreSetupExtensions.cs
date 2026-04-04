using LexiCore.Data;
using LexiCore.Models;
using LexiCore.Services;
using LexiCore.Services.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;

namespace LexiCore.Extensions;

/// <summary>
/// Provides extension methods for configuring and initializing the LexiCore library
/// in an ASP.NET Core application.
/// </summary>
public static class LexiCoreSetupExtensions
{
  /// <summary>
  /// Adds LexiCore services and configuration to the specified IServiceCollection.
  /// </summary>
  /// <param name="services">The IServiceCollection to which the LexiCore services will be added.</param>
  /// <param name="setup">
  /// An Action to configure the <see cref="Options"/>.
  /// This allows customization of options such as database configuration and supported cultures.
  /// </param>
  /// <returns>
  /// The IServiceCollection to allow for method chaining.
  /// </returns>
  public static IServiceCollection AddLexiCore(this IServiceCollection services, Action<Options> setup)
  {
    var options = new Options();
    setup(options);

    services.AddSingleton(options);
    services.AddMemoryCache();
    if (options.ConfigureDbContext != null)
      services.AddDbContext<ITranslationDbContext, TranslationDbContext>(options.ConfigureDbContext);

    services.AddScoped<ITranslationService, TranslationService>();
    services.AddSingleton<IStringLocalizerFactory, TranslationStringLocalizerFactory>();
    services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
    return services;
  }

  /// <summary>
  /// Initializes the LexiCore database by ensuring it is created.
  /// </summary>
  /// <param name="app">
  /// The <see cref="IApplicationBuilder"/> used to access application services
  /// and manage the database initialization process.
  /// </param>
  /// <returns>
  /// A <see cref="Task"/> representing the asynchronous operation.
  /// </returns>
  public static async Task InitializeLexiCoreDatabaseAsync(this IApplicationBuilder app)
  {
    await using var scope = app.ApplicationServices.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ITranslationDbContext>();
    await db.Database.EnsureCreatedAsync();
  }

  /// <summary>
  /// Maps the LexiCore API endpoints to the specified <see cref="IEndpointRouteBuilder"/>.
  /// </summary>
  /// <param name="endpoints">
  /// The <see cref="IEndpointRouteBuilder"/> to which the LexiCore API endpoints will be added.
  /// </param>
  /// <returns>
  /// The <see cref="IEndpointRouteBuilder"/> with the LexiCore API endpoints configured.
  /// </returns>
  public static IEndpointRouteBuilder MapLexiCoreApi(this IEndpointRouteBuilder endpoints)
  {
    var group = endpoints.MapGroup("/api/lexi-core");
    
    var options = endpoints.ServiceProvider.GetRequiredService<Options>();
    if (options.RequireAuthorization)
    {
      if (!string.IsNullOrEmpty(options.AuthorizationPolicy))
      {
        group.RequireAuthorization(options.AuthorizationPolicy);
      }
      else
      {
        group.RequireAuthorization();
      }
    }
    
    group.MapGet("/cultures", (Options opt) => Results.Ok(opt.SupportedCultures.Select(info => new { code = info.Name, name = info.NativeName })));
    group.MapGet(string.Empty, async (ITranslationService s) => Results.Ok(await s.GetAllAsync()));
    group.MapPost(string.Empty, async (LexiCoreEntry entry, ITranslationService s) =>
    {
      try
      {
        DotLiquid.Template.Parse(entry.Value);
      }
      catch (Exception ex)
      {
        return Results.BadRequest(new { error = ex.Message });
      }

      await s.UpsertAsync(entry);
      return Results.Ok();
    });
    group.MapDelete("/{key}/{culture}", async (string key, string culture, ITranslationService s) =>
    {
      await s.DeleteAsync(key, culture);
      return Results.Ok();
    });
    return endpoints;
  }

  /// <summary>
  /// Configures the application to serve the LexiCore administrative UI from a specified path.
  /// </summary>
  /// <param name="app">The application builder used to configure the pipeline.</param>
  /// <param name="path">
  /// The request path where the LexiCore UI will be hosted. Defaults to "/lexiCore-admin".
  /// </param>
  /// <returns>
  /// The <see cref="IApplicationBuilder"/> to allow for method chaining.
  /// </returns>
  public static IApplicationBuilder UseLexiCoreUi(this IApplicationBuilder app, string path = "/lexi-core-ui")
  {
    var assembly = typeof(LexiCoreSetupExtensions).Assembly;
    var fileProvider = new ManifestEmbeddedFileProvider(assembly, "ClientApp/dist");

    app.UseStaticFiles(new StaticFileOptions
    {
      FileProvider = fileProvider,
      RequestPath = path
    });

    app.Map(path, builder =>
    {
      builder.Run(async context =>
      {
        context.Response.ContentType = "text/html";
        var indexFile = fileProvider.GetFileInfo("index.html");
        await using var stream = indexFile.CreateReadStream();
        using var reader = new StreamReader(stream);
        await context.Response.WriteAsync(await reader.ReadToEndAsync());
      });
    });

    return app;
  }
}