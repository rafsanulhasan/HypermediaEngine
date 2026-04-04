namespace HypermediaEngine.Tests.Builders;

using HypermediaEngine.Builders;
using FluentAssertions;
using Xunit;

public class HypermediaCollectionBuilderTests
{
    private record Product(int Id, string Name, decimal Price);

    [Fact]
    public void Build_ShouldReturnResponseWithItems()
    {
        var products = new[] { new Product(1, "Widget", 9.99m), new Product(2, "Gadget", 19.99m) };
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(products);

        var response = builder.Build();

        response.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Build_ShouldHandleEmptyItems()
    {
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(Array.Empty<Product>());

        var response = builder.Build();

        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public void Build_ShouldHandleNullItems()
    {
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(null!);

        var response = builder.Build();

        response.Items.Should().BeEmpty();
    }

    [Fact]
    public void WithMetadata_ShouldSetPaginationInfo()
    {
        var builder = new HypermediaCollectionBuilder<Product>()
            .WithItems(Array.Empty<Product>())
            .WithMetadata(100, 2, 10);

        var response = builder.Build();

        response.Metadata.Should().NotBeNull();
        response.Metadata!.TotalCount.Should().Be(100);
        response.Metadata.Page.Should().Be(2);
        response.Metadata.PageSize.Should().Be(10);
        response.Metadata.TotalPages.Should().Be(10);
        response.Metadata.HasNextPage.Should().BeTrue();
        response.Metadata.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void WithMetadata_FirstPageShouldNotHavePreviousPage()
    {
        var builder = new HypermediaCollectionBuilder<Product>()
            .WithItems(Array.Empty<Product>())
            .WithMetadata(100, 1, 10);

        var response = builder.Build();

        response.Metadata!.HasPreviousPage.Should().BeFalse();
        response.Metadata.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void WithMetadata_LastPageShouldNotHaveNextPage()
    {
        var builder = new HypermediaCollectionBuilder<Product>()
            .WithItems(Array.Empty<Product>())
            .WithMetadata(100, 10, 10);

        var response = builder.Build();

        response.Metadata!.HasNextPage.Should().BeFalse();
        response.Metadata.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void TotalCount_ShouldDefaultToItemCountWhenNoMetadata()
    {
        var products = new[] { new Product(1, "Widget", 9.99m), new Product(2, "Gadget", 19.99m) };
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(products);

        var response = builder.Build();

        response.TotalCount.Should().Be(2);
        response.Metadata.Should().BeNull();
    }

    [Fact]
    public void WithLink_ShouldAddLinks()
    {
        var builder = new HypermediaCollectionBuilder<Product>()
            .WithItems(Array.Empty<Product>())
            .WithLink("self", "/products")
            .WithLink("next", "/products?page=2");

        var response = builder.Build();

        response.Links.Should().HaveCount(2);
        response.Links["self"].Href.Should().Be("/products");
        response.Links["next"].Href.Should().Be("/products?page=2");
    }
}
