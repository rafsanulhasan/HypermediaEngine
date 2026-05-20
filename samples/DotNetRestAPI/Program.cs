using Asp.Versioning;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;

using Bogus;

using DotNetRestAPI;
using DotNetRestAPI.Controllers;

using HypermediaEngine;
using HypermediaEngine.OpenApi;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

builder.Services.AddTransient(_ => TimeProvider.System);

string? connectionString = builder.Configuration.GetConnectionString("DB");

//builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddNpgsql<AppDbContext>(
    connectionString,
    npgsqlBuilder =>
    {
        npgsqlBuilder.UseParameterizedCollectionMode(ParameterTranslationMode.MultipleParameters);
        npgsqlBuilder.CommandTimeout(600);
    },
    dbCtxBuilder =>
    {
        dbCtxBuilder.EnableSensitiveDataLogging(true);
        dbCtxBuilder.EnableDetailedErrors(true);
    });
builder.Services.AddRouting(opttions =>
{
    opttions.AppendTrailingSlash = false;
    opttions.LowercaseUrls = true;
    opttions.LowercaseQueryStrings = true;
});
builder
    .Services
    .AddControllers(options =>
    {

    })
    .AddControllersAsServices();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .AddOpenApi("v1", options =>
    {
        options.AddScalarTransformers();
        options.RegisterHypermediaTransformers();
        options.AddDocumentTransformer((doc, ctx, ct) =>
        {
            doc.Info.Title = ".NETRestAPI";
            doc.Info.Version = "v1.0";
            return Task.CompletedTask;
        });
    })
    .AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1);
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            //new UrlSegmentApiVersionReader(),
            new QueryStringApiVersionReader("v"),
            new HeaderApiVersionReader("X-API-Version"),
            new MediaTypeApiVersionReader("v")
        );
        options.UnsupportedApiVersionStatusCode = StatusCodes.Status505HttpVersionNotsupported;
    })
    .AddMvc(options =>
    {
        IControllerConventionBuilder<WeatherController> controllerConventionBuilder = options
            .Conventions
            .Controller<WeatherController>();
        controllerConventionBuilder.HasApiVersions([new(1)]);
        controllerConventionBuilder.AdvertisesApiVersions([new(1, 0)]);
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
        options.DefaultApiVersion = new ApiVersion(1);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.RouteConstraintName = "version";
    });

builder.Services.RegisterHypermediaEngineToEndpoints();
builder.Services.AddEndpointsApiExplorer();
WebApplication app = builder.Build();

await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.EnsureCreatedAsync();

//app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

ApiVersionSet apiVersionSet = app
    .NewApiVersionSet("v1")
    .HasApiVersions([new(1)])
    .AdvertisesApiVersions([new(1)])
    .ReportApiVersions()
    .Build();

app
    .MapPost("/api/endpoints/weather/array", (TimeProvider timeProvider, [FromQuery] int count = 100) =>
    {
        Faker<WeatherForecast> faker = new();
        faker.RuleFor(w => w.Date, f => timeProvider.GetUtcNow())
             .RuleFor(w => w.TemperatureC, f => f.Random.Int(-20, 1))
             .RuleFor(w => w.Summary, f => f.Lorem.Sentence(3));
        Faker<WeatherForecast> faker2 = new();
        faker2.RuleFor(w => w.Date, f => timeProvider.GetUtcNow())
             .RuleFor(w => w.TemperatureC, f => f.Random.Int(1, 20))
             .RuleFor(w => w.Summary, f => f.Lorem.Sentence(3));
        Faker<WeatherForecast> faker3 = new();
        faker3.RuleFor(w => w.Date, f => timeProvider.GetUtcNow())
             .RuleFor(w => w.TemperatureC, f => f.Random.Int(21, 40))
             .RuleFor(w => w.Summary, f => f.Lorem.Sentence(3));
        Faker<WeatherForecast> faker4 = new();
        faker4.RuleFor(w => w.Date, f => timeProvider.GetUtcNow())
             .RuleFor(w => w.TemperatureC, f => f.Random.Int(41, 55))
             .RuleFor(w => w.Summary, f => f.Lorem.Sentence(3));
        Faker<WeatherForecast> faker5 = new();
        faker5.RuleFor(w => w.Date, f => timeProvider.GetUtcNow())
             .RuleFor(w => w.TemperatureC, f => f.Random.Int(60, 75))
             .RuleFor(w => w.Summary, f => f.Lorem.Sentence(3));

        int perChunk = (int)Math.Ceiling((double)count / 5);

        WeatherForecast[] response =
        [
            ..faker.Generate(perChunk),
            ..faker2.Generate(perChunk),
            ..faker3.Generate(perChunk),
            ..faker4.Generate(perChunk),
            ..faker5.Generate(perChunk),
        ];

        return response;
    })
    .ProducesJsonHal<WeatherForecast>(isList: true)
    .WithPagingParams()
    .WithFilterAndSortingParams()
    .WithName("Weather")
    .WithDescription("Descriptive Weather Info")
    .WithSummary("Summarized Weather Info")
    .WithApiVersionSet(apiVersionSet)
    .MapToApiVersion(1);

app.MapOpenApi("/api/docs/{documentName}.json");

app.MapScalarApiReference("/api/docs", options =>
{
    options.AddDocument("v1", ".NET API v1.0", isDefault: true);
    options.WithOpenApiRoutePattern("/api/docs/{documentName}.json");
});

await app.RunAsync().ConfigureAwait(false);
