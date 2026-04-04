namespace HypermediaEngine.Tests.Services;

using HypermediaEngine.Builders;
using HypermediaEngine.Interfaces;
using HypermediaEngine.Services;
using FluentAssertions;
using Xunit;

public class HypermediaServiceTests
{
    private readonly IHypermediaService _service;

    public HypermediaServiceTests()
    {
        _service = new HypermediaService();
    }

    private record Product(int Id, string Name, decimal Price);

    [Fact]
    public void CreateResponse_ShouldReturnHypermediaResponseWithData()
    {
        var product = new Product(1, "Widget", 9.99m);

        var response = _service.CreateResponse(product, builder =>
            builder.WithLink("self", "/products/1"));

        response.Data.Should().Be(product);
        response.Links.Should().ContainKey("self");
        response.Links["self"].Href.Should().Be("/products/1");
    }

    [Fact]
    public void CreateResponse_ShouldIncludeMultipleLinks()
    {
        var product = new Product(1, "Widget", 9.99m);

        var response = _service.CreateResponse(product, builder =>
        {
            builder
                .WithLink("self", "/products/1")
                .WithLink("update", "/products/1", "PUT")
                .WithLink("delete", "/products/1", "DELETE");
        });

        response.Links.Should().HaveCount(3);
        response.Links["update"].Method.Should().Be("PUT");
        response.Links["delete"].Method.Should().Be("DELETE");
    }

    [Fact]
    public void CreateCollectionResponse_ShouldReturnCollectionWithItems()
    {
        var products = new[]
        {
            new Product(1, "Widget", 9.99m),
            new Product(2, "Gadget", 19.99m)
        };

        var response = _service.CreateCollectionResponse(products, builder =>
        {
            builder
                .WithLink("self", "/products")
                .WithMetadata(2, 1, 10);
        });

        response.Items.Should().HaveCount(2);
        response.Links.Should().ContainKey("self");
        response.Metadata.Should().NotBeNull();
        response.Metadata!.TotalCount.Should().Be(2);
    }

    [Fact]
    public void CreateCollectionResponse_ShouldSupportPagination()
    {
        var products = Enumerable.Range(1, 10).Select(i => new Product(i, $"Product {i}", i * 10m));

        var response = _service.CreateCollectionResponse(products, builder =>
        {
            builder
                .WithLink("self", "/products?page=2")
                .WithLink("prev", "/products?page=1")
                .WithLink("next", "/products?page=3")
                .WithMetadata(100, 2, 10);
        });

        response.Metadata!.Page.Should().Be(2);
        response.Metadata.PageSize.Should().Be(10);
        response.Metadata.TotalCount.Should().Be(100);
        response.Metadata.TotalPages.Should().Be(10);
        response.Metadata.HasNextPage.Should().BeTrue();
        response.Metadata.HasPreviousPage.Should().BeTrue();
    }
}
