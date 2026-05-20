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

namespace HypermediaEngine.IntegrationTests.HypermediaEngine.Endpoints;

internal sealed class WeatherEndpointTests : TestBase
{
    private HttpClient _httpClient = null!;
    private CancellationTokenSource _cancellationTokenSource = null!;

    [SetUp]
    public void InitializeTest()
    {
        _cancellationTokenSource = new();
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
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        HypermediaCollectionResponse<WeatherForecast>? halResponse = await response.Content
            .ReadFromJsonAsync<HypermediaCollectionResponse<WeatherForecast>>(
                _cancellationTokenSource.Token);
        halResponse.ShouldNotBeNull();
        halResponse!.Items.Count().ShouldBe(10);
        halResponse.Meta.ShouldNotBeNull();
        halResponse.Meta!.Paging.ShouldNotBeNull();
        halResponse.Meta.Paging!.PageSize.ShouldBe(10);
        halResponse.Meta.Paging!.HasNext.ShouldBe(true);
        halResponse.Meta.Paging!.Style.ShouldBe(PagingStyles.Offset);
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
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        HypermediaCollectionResponse<WeatherForecast>? halResponse = await response.Content
            .ReadFromJsonAsync<HypermediaCollectionResponse<WeatherForecast>>(
                _cancellationTokenSource.Token);
        halResponse.ShouldNotBeNull();
        halResponse!.Items.Count().ShouldBe(10);
        halResponse.Meta.ShouldNotBeNull();
        halResponse.Meta!.Paging.ShouldNotBeNull();
        halResponse.Meta.Paging!.TotalCount.ShouldBe(20);
        halResponse.Meta.Paging!.PageSize.ShouldBe(10);
        halResponse.Meta.Paging!.HasNext.ShouldBe(true);
        halResponse.Meta.Paging!.Style.ShouldBe(PagingStyles.Offset);
    }

    [TearDown]
    public void EndTest()
    {
        _httpClient.Dispose();
        _cancellationTokenSource.Dispose();
    }
}
