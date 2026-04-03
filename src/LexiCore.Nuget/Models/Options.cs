using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace LexiCore.Models;

/// <summary>
/// Encapsulates configuration options for the LexiCore library.
/// </summary>
public class Options
{
  /// <summary>
  /// Configures the database context options for the application by allowing the specification of an action
  /// that modifies the behavior and configuration of the <see cref="Microsoft.EntityFrameworkCore.DbContextOptionsBuilder"/>.
  /// This property is used to set up the database provider, connection string, or other context-specific settings
  /// needed for the application's data access layer.
  /// </summary>
  public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; set; }

  /// <summary>
  /// Specifies the list of cultures that the application supports for localization and internationalization purposes.
  /// Each culture is represented as an instance of the <see cref="System.Globalization.CultureInfo"/> class,
  /// defining language and regional settings such as formatting, sorting, and language-specific text.
  /// This property is used to configure the available cultures for the application's multilingual features.
  /// </summary>
  public List<CultureInfo> SupportedCultures { get; set; } = [new("en-US")];

  /// <summary>
  /// Indicates whether authorization is required to access specific routes or endpoints within the application.
  /// When set to <c>true</c>, the application will enforce access control policies as defined by the
  /// <see cref="AuthorizationPolicy"/> property or other default configurations. Setting this property to <c>false</c>
  /// disables authorization checks for the relevant routes or endpoints.
  /// </summary>
  public bool RequireAuthorization { get; set; } = false;

  /// <summary>
  /// Gets or sets the name of the authorization policy to be applied when securing routes or endpoints.
  /// This property allows you to specify a predefined authorization policy that will be used to enforce
  /// access control for resources within the application. If the policy is set and <see cref="Options.RequireAuthorization"/>
  /// is enabled, the specified policy will be applied to the corresponding routes or endpoints.
  /// </summary>
  public string? AuthorizationPolicy { get; set; }
}