//using Humanizer;

//using Microsoft.Extensions.DependencyInjection;

//namespace HypermediaEngine.IntegrationTests.Fixtures;

///// <inheritdoc />
//public class AspireFixture : AspireFixture<Projects.Sample_AppHost>
//{
//    /// <inheritdoc />
//    protected override TimeSpan ResourceTimeout => 3.Minutes();

//    /// <inheritdoc />
//    protected override ResourceWaitBehavior WaitBehavior => ResourceWaitBehavior.None;

//    protected override string[] Args => ["--environment", "Test"];

//    /// <inheritdoc />
//    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
//    {
//        builder.Environment.EnvironmentName = "Test";
//        // Configure the builder before the app is built
//        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
//        {
//            clientBuilder.AddStandardResilienceHandler();
//        });
//    }

//    /// <inheritdoc />
//    protected override IEnumerable<string> ResourcesToRemove()
//    {
//        return ["Postgres"];
//    }
//}