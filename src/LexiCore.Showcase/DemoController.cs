using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace LexiCore.Showcase;

[ApiController]
[Route("api/[controller]")]
public class DemoController(IStringLocalizer<DemoController> localizer) : ControllerBase
{
  [HttpGet("greet/{name}")]
  public IActionResult GetGreeting(string name) =>
    Ok(new {
      culture = System.Globalization.CultureInfo.CurrentUICulture.Name,
      rendered_html = localizer["welcome_header", new
      {
        name,
        array = new[] { "item1", "item2" }
      }].Value
    });
}