using Bogus;

using Microsoft.Extensions.Hosting;

using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgresResource = builder
    .AddPostgres(
        "Postgres",
        userName: builder.AddResource<ParameterResource>(new("USERNAME", d => "postgres")),
        password: builder.AddResource<ParameterResource>(new("PASSWORD", d => "postgres")),
        port: 4432);

bool isTestEnv = builder.Environment.IsEnvironment("Test");

if (isTestEnv)
{
    postgresResource = postgresResource
        // 🔥 Speed-up Arguments: Disables safety flushing to disk for lightning-fast test execution
        .WithArgs(
            "-c", "fsync=off",
            "-c", "synchronous_commit=off",
            "-c", "full_page_writes=off");
}

postgresResource = postgresResource.PublishAsContainer();
if (isTestEnv == false)
    postgresResource = postgresResource.WithDataVolume("restapi-vol");

IResourceBuilder<PostgresDatabaseResource> postgresDbResource = isTestEnv
    ? postgresResource.AddDatabase("DB", new Faker().Name.LastName())
    : postgresResource.AddDatabase("DB", "RestAPI");
if (isTestEnv)
    postgresDbResource = postgresDbResource.WithCreationScript($"CREATE DATABASE {postgresDbResource.Resource.DatabaseName}");

IResourceBuilder<ProjectResource> apiResource = builder
    .AddProject<Projects.DotNetRestAPI>("dotnetrestapi")
    .WithReference(postgresDbResource);

if (isTestEnv is false)
{
    apiResource.WaitFor(postgresDbResource);
}

builder.Build().Run();
