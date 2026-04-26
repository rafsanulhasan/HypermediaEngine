# HypermediaEngine

A .NET library for building HATEOAS-compliant REST APIs. Automatically enriches API responses with hypermedia links, metadata, and optional ETag-based HTTP caching.

Supports objects, arrays, and queryables (with built-in filtering, sorting, and paging).

---

## Features

- Automatic `_links` generation (self, related, state transitions, pagination)
- Response metadata: API version, ETag, entity version
- Built-in filtering, sorting, and paging for collection endpoints
- ETag middleware: SHA-256 hash, `If-None-Match` / `304 Not Modified` support
- OpenAPI / Scalar integration with HAL+JSON and JSON+HAL schema transformers
- Works with both Minimal APIs and MVC controllers
- Fluent builder API and handler chain extensibility

---

## Solution Structure

```
src/
  HypermediaEngine/
    Core/               # Domain models: responses, links, filters, paging, sorting
    AspNetCore/         # DI registration, builders, handlers, OpenAPI transformers
    AspNetCore.Mvc/     # MVC controller support (in progress)
  EntityTagCaching/
    Core/               # ETag abstractions
    AspNetCore/         # ETag middleware and SHA-256 service
    CodeGeneration/     # Source generators for ETag support
samples/
  DotNetRestAPI/        # Minimal API + MVC controller demo
  AppHost/              # .NET Aspire orchestration host
  ServiceDefaults/      # Shared Aspire service defaults
tests/
  HypermediaEngine.Tests/
```

---

## Getting Started

### 1. Register services

```csharp
// Program.cs
builder.Services.RegisterHypermediaEngineToEndpoints();
```

To customize registration (e.g. supply a custom link generation service):

```csharp
builder.Services.RegisterHypermediaEngineToEndpoints(cfg =>
    cfg.WithLinkGenerationService(sp => new MyLinkGenerationService(sp))
       .WithRouteOptions(opts => opts.LowercaseUrls = true)
);
```

### 2. Register OpenAPI transformers (optional)

```csharp
builder.Services.AddOpenApi("v1", options =>
{
    options.RegisterHypermediaTransformers(); // HAL+JSON / JSON+HAL schemas
    options.AddScalarTransformers();
});
```

---

## Usage

### Minimal API — collection endpoint

```csharp
app.MapPost("/api/weather", (TimeProvider timeProvider) =>
    {
        return Enumerable.Range(1, 20)
            .Select(_ => new WeatherForecast { /* ... */ })
            .ToArray();
    })
    .ProducesJsonHal<WeatherForecast>(isList: true)
    .WithPagingParams()
    .WithFilterAndSortingParams()
    .WithName("WeatherList");
```

### MVC controller

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class WeatherController(TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<WeatherForecast>>(StatusCodes.Status200OK)]
    public List<WeatherForecast> Get() => [ /* ... */ ];
}
```

### Manual response building via `IHypermediaService`

```csharp
public class WeatherService(IHypermediaService hypermedia)
{
    public HypermediaObjectResponse<WeatherForecast> GetForecast(int id)
    {
        WeatherForecast forecast = /* ... */;
        return hypermedia.CreateResponse(forecast);
    }

    public HypermediaCollectionResponse<WeatherForecast> GetForecasts(IEnumerable<WeatherForecast> items)
    {
        return hypermedia.CreateCollectionResponse(items);
    }
}
```

---

## ETag Caching

Add the middleware to enable HTTP caching via `ETag` / `If-None-Match`:

```csharp
app.UseMiddleware<ETagMiddleware>();
```

On GET requests the middleware:
1. Buffers the response body
2. Generates a SHA-256 `ETag` header
3. Compares against the `If-None-Match` request header
4. Returns `304 Not Modified` when the content is unchanged

---

## Response Shape

Every response follows the `{ data, error }` convention:

```json
{
  "data": {
    "date": "2025-07-01T00:00:00Z",
    "temperatureC": 22,
    "summary": "Warm"
  },
  "_links": {
    "self":   { "href": "/api/weather/1", "method": "GET" },
    "update": { "href": "/api/weather/1", "method": "PUT" },
    "delete": { "href": "/api/weather/1", "method": "DELETE" }
  },
  "_metadata": {
    "apiVersion": "1.0",
    "etag": "\"abc123\""
  }
}
```

Collection responses additionally include `_paging`, filter, and sort metadata, plus `next`, `previous`, `first`, and `last` pagination links.

---

## Building and Testing

```bash
dotnet build          # Build the solution
dotnet test           # Run unit tests
dotnet stryker        # Run mutation tests
dotnet run --project samples/DotNetRestAPI   # Run the sample API
```

The sample API serves Scalar UI at `/api/docs`.

---

## Requirements

- .NET 10.0
- ASP.NET Core 10.0
