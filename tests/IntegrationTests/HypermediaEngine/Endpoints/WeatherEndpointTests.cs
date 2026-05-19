using DotNetRestAPI;

using HypermediaEngine.Http;
using HypermediaEngine.IntegrationTests.Abstractions;
using HypermediaEngine.Requests;
using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Responses;

using Microsoft.Net.Http.Headers;

using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;

namespace HypermediaEngine.IntegrationTests.HypermediaEngine.Endpoints;

//[ClassDataSource<AspireFixture>(Shared = SharedType.PerClass)]
//internal sealed class WeatherEndpointTests(AspireFixture fixture)
internal sealed class WeatherEndpointTests : TestBase
{
    private HttpClient _httpClient;
    private CancellationTokenSource _cancellationTokenSource;

    [Before(Test)]
    public async Task InitializeTest()
    {
        _cancellationTokenSource = new();
        //await fixture.App.ResourceNotifications
        //    .WaitForResourceAsync("dotnetrestapi")
        //    .WaitAsync(_cancellationTokenSource.Token);
        //_httpClient = fixture.CreateHttpClient("dotnetrestapi");
        _httpClient = Factory.CreateClient();
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithNoBody_ReturnsHalCollectionResponse()
    {
        // Arrange
        _httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Accept,
            HalMediaTypeNames.Application.HalJson);

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync<QueryBody?>(
            "/api/endpoints/weather/array",
            null,
            _cancellationTokenSource.Token);

        // Assert
        using (Assert.Multiple())
        {
            await response.StatusCode.Should().BeEqualTo(HttpStatusCode.OK);
            await Assert.That(async () =>
            {
                HypermediaCollectionResponse<WeatherForecast>? halResponse = await response.Content
                    .ReadFromJsonAsync<HypermediaCollectionResponse<WeatherForecast>>(
                        _cancellationTokenSource.Token);
                await halResponse.Should().NotBeNull();
                await halResponse!.Items.Should().HaveCount(10);
                await halResponse.Meta.Should().NotBeNull();
                await halResponse.Meta!.Paging.Should().NotBeNull();
                await halResponse.Meta.Paging!.PageSize
                    .Should().BeEqualTo(10);
                await halResponse.Meta.Paging!.HasNext
                    .Should().BeTrue();
                await halResponse.Meta.Paging!.Style
                    .Should().BeEqualTo(PagingStyles.Offset);

            }).ThrowsNothing();
        }
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithFilter_ReturnsHalCollectionResponse()
    {
        // Arrange
        _httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Accept,
            HalMediaTypeNames.Application.HalJson);
        QueryBody queryBody = new()
        {
            Filtering = new([new("TemperatureC", FilterOperator.Gte, 60)])
        };

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "/api/endpoints/weather/array",
            queryBody,
            QueryParamsSerializerContext.Default.QueryBody,
            _cancellationTokenSource.Token);

        // Assert
        using (Assert.Multiple())
        {
            await response.StatusCode.Should().BeEqualTo(HttpStatusCode.OK);
            await Assert.That(async () =>
            {
                HypermediaCollectionResponse<WeatherForecast>? halResponse = await response.Content
                    .ReadFromJsonAsync<HypermediaCollectionResponse<WeatherForecast>>(
                        _cancellationTokenSource.Token);
                await halResponse.Should().NotBeNull();
                await halResponse!.Items.Should().HaveCount(10);
                await halResponse.Meta.Should().NotBeNull();
                await halResponse.Meta!.Paging.Should().NotBeNull();
                await halResponse.Meta.Paging!.TotalCount
                    .Should().BeEqualTo(20);
                await halResponse.Meta.Paging!.PageSize
                    .Should().BeEqualTo(10);
                await halResponse.Meta.Paging!.HasNext
                    .Should().BeTrue();
                await halResponse.Meta.Paging!.Style
                    .Should().BeEqualTo(PagingStyles.Offset);

            }).ThrowsNothing();
        }
    }

    [After(Test)]
    public void EndTest()
    {
        _httpClient.Dispose();
        _cancellationTokenSource.Dispose();
    }
}
