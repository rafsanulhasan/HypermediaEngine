using Asp.Versioning;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;

using Bogus;

using DotNetRestAPI;
using DotNetRestAPI.Controllers;

using HypermediaEngine;
using HypermediaEngine.OpenApi;
using HypermediaEngine.Requests;

using LanguageExt;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

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

app.MapDefaultEndpoints();

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
    .MapPost("/api/endpoints/weather", (TimeProvider timeProvider) =>
    {
        Faker<WeatherForecast> faker = new();
        faker.RuleFor(w => w.Date, f => timeProvider.GetUtcNow().AddDays(f.Random.Int(1, 30)))
             .RuleFor(w => w.TemperatureC, f => f.Random.Int(-20, 55))
             .RuleFor(w => w.Summary, f => f.Lorem.Sentence(3));

        return Enumerable.Range(1, 20).Select(_ => faker.Generate()).ToArray();
    })
    .ProducesHal<WeatherForecast>(StatusCodes.Status200OK, isList: true)
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
