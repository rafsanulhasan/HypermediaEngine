using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;

namespace DotNetRestAPI.Controllers;

/// <summary>
/// Weather Api
/// </summary>
[ApiController]
//[Route("api/controllers/[controller]", Name = "WeatherControllerDefault")]
[Route("api/v{version:apiVersion}/controllers/[controller]", Name = "WeatherControllerVersioned")]
[ApiVersion("1.0")]
public class WeatherController(TimeProvider timeProvider) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "/")]
    [ProducesResponseType<List<WeatherForecast>>(StatusCodes.Status200OK, contentType: MediaTypeNames.Application.Json)]
    public List<WeatherForecast> Get()
    {
        return [.. Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = timeProvider.GetUtcNow(),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })];
    }
}
