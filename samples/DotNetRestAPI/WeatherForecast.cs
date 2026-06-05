namespace DotNetRestAPI;

public sealed record class WeatherForecast
{
    public Guid Id { get; set; }
    public DateTimeOffset Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }

    public bool IsCold => TemperatureC < 10;
    public bool IsHot => TemperatureC > 40;
    public bool IsWarm => TemperatureC >= 10
                       && TemperatureC <= 40;
}
