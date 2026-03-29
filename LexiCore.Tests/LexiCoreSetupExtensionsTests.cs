using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using LexiCore.Data;
using LexiCore.Extensions;
using LexiCore.Models;
using LexiCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiCore.Tests;

public class LexiCoreSetupExtensionsTests
{
  [Fact]
  public void AddLexiCore_ShouldRegisterAllRequiredServices()
  {
    var services = new ServiceCollection();

    services.AddLexiCore(options => options.ConfigureDbContext = builder => builder.UseInMemoryDatabase("TestDb"));

    var provider = services.BuildServiceProvider();

    Assert.NotNull(provider.GetService<Options>());
    Assert.NotNull(provider.GetService<ITranslationDbContext>());
    Assert.NotNull(provider.GetService<ITranslationService>());
    Assert.NotNull(provider.GetService<IStringLocalizerFactory>());
  }

  [Fact]
  public async Task InitializeLexiCoreDatabaseAsync_ShouldEnsureDatabaseIsCreated()
  {
    var services = new ServiceCollection();
    services.AddDbContext<ITranslationDbContext, TranslationDbContext>(builder => builder.UseInMemoryDatabase("InitTestDb"));
    var provider = services.BuildServiceProvider();

    var appMock = Substitute.For<IApplicationBuilder>();
    appMock.ApplicationServices.Returns(provider);

    var exception = await Record.ExceptionAsync(() => appMock.InitializeLexiCoreDatabaseAsync());
    Assert.Null(exception);
  }
  
  [Fact]
  public async Task MapLexiCoreApi_GetCultures_ShouldReturnConfiguredCultures()
  {
    var mockService = Substitute.For<ITranslationService>();
    var options = new Options
    {
      SupportedCultures = [new("en-US"), new CultureInfo("fr-FR")]
    };

    using var host = await CreateTestHostAsync(mockService, options);
    var client = host.GetTestClient();

    var response = await client.GetAsync("/api/lexi-core/cultures");

    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("en-US", content);
    Assert.Contains("fr-FR", content);
  }

  [Fact]
  public async Task MapLexiCoreApi_Post_ShouldReturnOk_WhenDotLiquidTemplateIsValid()
  {
    var mockService = Substitute.For<ITranslationService>();
    var options = new Options();

    using var host = await CreateTestHostAsync(mockService, options);
    var client = host.GetTestClient();

    var validEntry = new Translation
    {
      Key = "welcome",
      Culture = "en-US",
      Value = "Hello {{ user.name }}!"
    };

    var response = await client.PostAsJsonAsync("/api/lexi-core", validEntry);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    await mockService.Received(1).UpsertAsync(Arg.Is<Translation>(translation => translation.Key == "welcome"));
  }

  [Fact]
  public async Task MapLexiCoreApi_Post_ShouldReturnBadRequest_WhenDotLiquidTemplateIsInvalid()
  {
    var mockService = Substitute.For<ITranslationService>();
    var options = new Options();

    using var host = await CreateTestHostAsync(mockService, options);
    var client = host.GetTestClient();

    var invalidEntry = new Translation
    {
      Key = "broken_template",
      Culture = "en-US",
      Value = "Hello {% if user.name %} Missing end tag!"
    };

    var response = await client.PostAsJsonAsync("/api/lexi-core", invalidEntry);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var errorContent = await response.Content.ReadAsStringAsync();
    Assert.Contains("error", errorContent);

    await mockService.DidNotReceiveWithAnyArgs().UpsertAsync(Arg.Any<Translation>());
  }

  [Fact]
  public async Task MapLexiCoreApi_Delete_ShouldCallDeleteAsyncAndReturnOk()
  {
    var mockService = Substitute.For<ITranslationService>();
    var options = new Options();

    using var host = await CreateTestHostAsync(mockService, options);
    var client = host.GetTestClient();

    var response = await client.DeleteAsync("/api/lexi-core/welcome_message/en-US");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    await mockService.Received(1).DeleteAsync("welcome_message", "en-US");
  }

  [Fact]
  public async Task UseLexiCoreUI_ShouldServeIndexHtml_OnBasePath()
  {
    using var host = await new HostBuilder()
      .ConfigureWebHost(webBuilder =>
      {
        webBuilder.UseTestServer();
        webBuilder.Configure(app =>
        {
          try
          {
            app.UseLexiCoreUi("/test-admin");
          }
          catch (InvalidOperationException ex) when (ex.Message.Contains("manifest"))
          {
            throw new Exception(
              "UI Test Failed: The embedded manifest was not found. " +
              "Ensure 'npm run build' executes and embeds the 'dist' folder before running tests.", ex);
          }
        });
      })
      .StartAsync();

    var client = host.GetTestClient();

    var response = await client.GetAsync("/test-admin");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    var html = await response.Content.ReadAsStringAsync();
    Assert.Contains("<html", html, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("<body", html, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task UseLexiCoreUI_ShouldServeIndexHtml_AsSpaFallbackForDeepLinks()
  {
    using var host = await new HostBuilder()
      .ConfigureWebHost(webBuilder =>
      {
        webBuilder.UseTestServer();
        webBuilder.Configure(app => app.UseLexiCoreUi("/test-admin"));
      })
      .StartAsync();

    var client = host.GetTestClient();

    var response = await client.GetAsync("/test-admin/settings/advanced");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

    var html = await response.Content.ReadAsStringAsync();
    Assert.Contains("<html", html, StringComparison.OrdinalIgnoreCase);
  }
  
  
  private static async Task<IHost> CreateTestHostAsync(ITranslationService mockService, Options options)
  {
    var hostBuilder = new HostBuilder()
      .ConfigureWebHost(webBuilder =>
      {
        webBuilder.UseTestServer();

        webBuilder.ConfigureServices(services =>
        {
          services.AddRouting();
          services.AddSingleton(mockService);
          services.AddSingleton(options);
        });

        webBuilder.Configure(app =>
        {
          app.UseRouting();
          app.UseEndpoints(endpoints => endpoints.MapLexiCoreApi());
        });
      });

    return await hostBuilder.StartAsync();
  }
}