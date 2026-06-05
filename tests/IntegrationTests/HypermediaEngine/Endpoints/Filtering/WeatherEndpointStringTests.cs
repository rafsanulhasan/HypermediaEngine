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
internal sealed class WeatherEndpointStringTests : TestBase
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
    public async Task RequestWeatherForecastWithHalJson_WithAndLogic_ReturnsOnlyMatchingTemperatures()
    {
        // Arrange
        QueryBody queryBody = new()
        {
            Filtering = new(
                FilterLogic.And,
                [
                    new("Summary", FilterOperator.StartsWith, "Lorem"), 
                    new("Summary", FilterOperator.EndsWith, "ispum"),
                ],
                children: null),
        };

        // Act
        HypermediaCollectionResponse<WeatherForecast>? halResponse = await PostAndReadHalResponseAsync(queryBody);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            halResponse.ShouldNotBeNull();
            halResponse.Items.Count().ShouldBe(10);
            AssertStandardOffsetPaging(halResponse, 40);
            foreach (WeatherForecast item in halResponse.Items)
            {
                item.Summary.ShouldStartWith("Lorem");
                item.Summary.ShouldEndWith("ispum");
            }
        }
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithChildrenOnlyOrFilter_ReturnsUnionOfChildNodes()
    {
        // Arrange
        FilterNode child2 = new(
            FilterLogic.And,
            [new("Summary", FilterOperator.StartsWith, "Lorem"), new("Summary", FilterOperator.EndsWith, "ispum")],
            children: null);
        QueryBody queryBody = new()
        {
            Filtering = new FilterNode(
                FilterLogic.Or, 
                [new("Summary", FilterOperator.NotContains, "abc")], 
                [child2]),
        };

        // Act
        HypermediaCollectionResponse<WeatherForecast>? halResponse =
            await PostAndReadHalResponseAsync(queryBody).ConfigureAwait(false);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            halResponse.ShouldNotBeNull();
            halResponse.Items.Count().ShouldBe(10);
            AssertStandardOffsetPaging(halResponse, 80);
            foreach (WeatherForecast item in halResponse.Items)
            {
                item.Summary.ShouldNotBeNull();
                (item.Summary == "Lorem" || (item.Summary.StartsWith("Lorem") && item.Summary.EndsWith("ispum")))
                    .ShouldBe(true);
            }
        }
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithFilter_ReturnsHalCollectionResponse()
    {
        // Arrange
        QueryBody queryBody = new()
        {
            Filtering = new([new("Summary", FilterOperator.Eq, "Lorem")]),
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
        }
    }

    [Test]
    public async Task RequestWeatherForecastWithHalJson_WithNestedAndOrFilter_PreservesChildGrouping()
    {
        // Arrange
        FilterNode child = new(
            FilterLogic.Or,
            [new("Summary", FilterOperator.NotStartsWith, "ispum"), new("Summary", FilterOperator.NotEndsWith, "Lorem")],
            children: null);
        QueryBody queryBody = new()
        {
            Filtering = new(
                FilterLogic.And,
                [new("Summary", FilterOperator.Contains, "last")],
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
                item.Summary.ShouldNotBeNull();
                (item.Summary.Contains("last") && (!item.Summary.StartsWith("ispum") || !item.Summary.EndsWith("Lorem")))
                    .ShouldBe(true);
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
        int expectedTotalCount)
    {
        response.Meta.ShouldNotBeNull();
        response.Meta!.Paging.ShouldNotBeNull();
        response.Meta.Paging!.TotalCount.ShouldBe(expectedTotalCount);
        response.Meta.Paging!.PageSize.ShouldBe(10);
        response.Meta.Paging!.HasNext.ShouldBe(true);
        response.Meta.Paging!.Style.ShouldBe(PagingStyles.Offset);
    }
}
