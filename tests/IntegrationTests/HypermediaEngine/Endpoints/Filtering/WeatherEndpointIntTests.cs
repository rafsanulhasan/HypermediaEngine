using DotNetRestAPI;

using Microsoft.Net.Http.Headers;

using SynergyFx.HypermediaEngine.Http;
using SynergyFx.HypermediaEngine.Requests;
using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.HypermediaEngine.Requests.Paging;
using SynergyFx.HypermediaEngine.Responses;
using SynergyFx.Tests.IntegrationTests.Abstractions;

using System.Net;
using System.Net.Http.Json;

namespace SynergyFx.Tests.IntegrationTests.HypermediaEngine.Endpoints.Filtering;

[Category("HypermediaEngine")]
internal sealed class WeatherEndpointIntTests : TestBase
{
    private HttpClient _httpClient = null!;
    private CancellationTokenSource _cancellationTokenSource = null!;

    [SetUp]
    public void InitializeTest()
    {
        _cancellationTokenSource = new();
        _httpClient = Factory.CreateClient();
        AddHalJsonAcceptHeader();
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithAndRangeFilter_ReturnsOnlyMatchingTemperatures()
    {
        // Arrange
        QueryBody queryBody = new()
        {
            Filtering = new(
                FilterLogic.And,
                [
                    new("TemperatureC", FilterOperator.Gte, 21), 
                    new("TemperatureC", FilterOperator.Lte, 40),
                ],
                children: null),
        };

        // Act
        HypermediaCollectionResponse<WeatherForecast>? halResponse = 
            await PostAndReadHalResponseAsync(queryBody).ConfigureAwait(false);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            halResponse.ShouldNotBeNull();
            halResponse.Items.Count().ShouldBe(10);
            AssertStandardOffsetPaging(halResponse, 20);
            foreach (WeatherForecast item in halResponse.Items)
            {
                (item.TemperatureC >= 21 && item.TemperatureC <= 40)
                    .ShouldBe(expected: true);
            }
        }
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithFilter_ReturnsHalCollectionResponse()
    {
        // Arrange
        QueryBody queryBody = new()
        {
            Filtering = new([new("TemperatureC", FilterOperator.Eq, 60)]),
        };

        // Act
        HypermediaCollectionResponse<WeatherForecast>? halResponse =
            await PostAndReadHalResponseAsync(queryBody).ConfigureAwait(false);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            halResponse.ShouldNotBeNull();
            halResponse.Items.Count().ShouldBe(1);
            AssertStandardOffsetPaging(halResponse, 1, 1, false);
        }
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithNestedAndOrFilter_PreservesChildGrouping()
    {
        // Arrange
        FilterNode child = new(
            FilterLogic.Or,
            [new("TemperatureC", FilterOperator.Eq, 60), new("TemperatureC", FilterOperator.Gte, 61)],
            children: null);
        QueryBody queryBody = new()
        {
            Filtering = new(
                FilterLogic.And,
                [new("TemperatureC", FilterOperator.Gte, 59)],
                [child]),
        };

        // Act
        HypermediaCollectionResponse<WeatherForecast>? halResponse =
            await PostAndReadHalResponseAsync(queryBody).ConfigureAwait(false);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            halResponse.ShouldNotBeNull();
            halResponse.Items.Count().ShouldBe(10);
            AssertStandardOffsetPaging(halResponse, 20);
            foreach (WeatherForecast item in halResponse.Items)
            {
                (item.TemperatureC > 60).ShouldBe(true);
            }
        }
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithNestedOrAndFilter_ReturnsUnionOfChildNodes()
    {
        // Arrange
        FilterNode child2 = new(
            FilterLogic.And,
            [new("TemperatureC", FilterOperator.Gt, 20), new("TemperatureC", FilterOperator.Lt, 41)],
            children: null);
        QueryBody queryBody = new()
        {
            Filtering = new FilterNode(FilterLogic.Or, [new("TemperatureC", FilterOperator.Gt, 59)], [child2]),
        };

        // Act
        HypermediaCollectionResponse<WeatherForecast>? halResponse =
            await PostAndReadHalResponseAsync(queryBody).ConfigureAwait(false);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            halResponse.ShouldNotBeNull();
            halResponse.Items.Count().ShouldBe(10);
            AssertStandardOffsetPaging(halResponse, 40);
            foreach (WeatherForecast item in halResponse.Items)
            {
                (item.TemperatureC > 60 || (item.TemperatureC > 20 && item.TemperatureC < 41)).ShouldBe(true);
            }
        }
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithNoBody_ReturnsHalCollectionResponse()
    {
        // Arrange

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync<QueryBody?>(
            "/api/endpoints/weather/array",
            value: null,
            _cancellationTokenSource.Token);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            HypermediaCollectionResponse<WeatherForecast>? halResponse = await response.Content
                .ReadFromJsonAsync<HypermediaCollectionResponse<WeatherForecast>>(
                    _cancellationTokenSource.Token);
            halResponse.ShouldNotBeNull();
            halResponse!.Items.Count().ShouldBe(10);
            halResponse.Meta.ShouldNotBeNull();
            halResponse.Meta!.Paging.ShouldNotBeNull();
            halResponse.Meta.Paging!.PageSize.ShouldBe(10);
            halResponse.Meta.Paging!.HasNext.ShouldBe(expected: true);
            halResponse.Meta.Paging!.Style.ShouldBe(PagingStyles.Offset);
        }
    }

    [TearDown]
    public void EndTest()
    {
        _httpClient.Dispose();
        _cancellationTokenSource.Dispose();
    }

    private void AddHalJsonAcceptHeader()
    {
        _httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Accept,
            HalMediaTypeNames.Application.HalJson);
    }

    private async Task<HypermediaCollectionResponse<WeatherForecast>?> PostAndReadHalResponseAsync(QueryBody queryBody)
    {
        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync(
                "/api/endpoints/weather/array",
                queryBody,
                QueryParamsSerializerContext.Default.QueryBody,
                _cancellationTokenSource.Token)
            .ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        HypermediaCollectionResponse<WeatherForecast>? halResponse = await response.Content
            .ReadFromJsonAsync<HypermediaCollectionResponse<WeatherForecast>>(
                _cancellationTokenSource.Token)
            .ConfigureAwait(false);
        return halResponse!;
    }

    private static void AssertStandardOffsetPaging(
        HypermediaCollectionResponse<WeatherForecast> response,
        int expectedTotalCount,
        int expectedPageSize = 10,
        bool expectedHasNext = true)
    {
        response.Meta.ShouldNotBeNull();
        response.Meta.Paging.ShouldNotBeNull();
        response.Meta.Paging.TotalCount.ShouldBe(expectedTotalCount);
        response.Meta.Paging.PageSize.ShouldBe(expectedPageSize);
        response.Meta.Paging.HasNext.ShouldBe(expectedHasNext);
        response.Meta.Paging.Style.ShouldBe(PagingStyles.Offset);
    }
}
