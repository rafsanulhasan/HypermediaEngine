namespace HypermediaEngine.Tests.Builders;

using HypermediaEngine.Builders;
using FluentAssertions;
using Xunit;

public class HypermediaBuilderTests
{
    private record Product(int Id, string Name, decimal Price);

    [Fact]
    public void Build_ShouldReturnResponseWithData()
    {
        var product = new Product(1, "Widget", 9.99m);
        var builder = new HypermediaBuilder<Product>().WithData(product);

        var response = builder.Build();

        response.Data.Should().Be(product);
    }

    [Fact]
    public void Build_ShouldReturnEmptyLinksWhenNoneAdded()
    {
        var builder = new HypermediaBuilder<Product>().WithData(new Product(1, "Widget", 9.99m));

        var response = builder.Build();

        response.Links.Should().BeEmpty();
    }

    [Fact]
    public void WithLink_ShouldAddLinkToResponse()
    {
        var builder = new HypermediaBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithLink("self", "/products/1");

        var response = builder.Build();

        response.Links.Should().ContainKey("self");
        response.Links["self"].Href.Should().Be("/products/1");
        response.Links["self"].Method.Should().Be("GET");
    }

    [Fact]
    public void WithLink_ShouldSupportCustomMethod()
    {
        var builder = new HypermediaBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithLink("update", "/products/1", "PUT", "Update Product");

        var response = builder.Build();

        response.Links["update"].Method.Should().Be("PUT");
        response.Links["update"].Title.Should().Be("Update Product");
    }

    [Fact]
    public void WithLink_ShouldOverwriteExistingLinkWithSameRel()
    {
        var builder = new HypermediaBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithLink("self", "/products/1")
            .WithLink("self", "/products/1-updated");

        var response = builder.Build();

        response.Links.Should().HaveCount(1);
        response.Links["self"].Href.Should().Be("/products/1-updated");
    }

    [Fact]
    public void WithLink_ShouldThrowForNullRel()
    {
        var builder = new HypermediaBuilder<Product>();
        var act = () => builder.WithLink(null!, "/products/1");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithLink_ShouldThrowForEmptyHref()
    {
        var builder = new HypermediaBuilder<Product>();
        var act = () => builder.WithLink("self", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithMetadata_ShouldAddMetadataToResponse()
    {
        var builder = new HypermediaBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithMetadata("version", "1.0")
            .WithMetadata("timestamp", "2024-01-01");

        var response = builder.Build();

        response.Metadata.Should().NotBeNull();
        response.Metadata.Should().ContainKey("version");
        response.Metadata!["version"].Should().Be("1.0");
    }

    [Fact]
    public void Build_ShouldReturnNullMetadataWhenNoneAdded()
    {
        var builder = new HypermediaBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m));

        var response = builder.Build();

        response.Metadata.Should().BeNull();
    }

    [Fact]
    public void WithLink_ShouldSupportFluentChaining()
    {
        var product = new Product(1, "Widget", 9.99m);
        var response = new HypermediaBuilder<Product>()
            .WithData(product)
            .WithLink("self", "/products/1")
            .WithLink("update", "/products/1", "PUT")
            .WithLink("delete", "/products/1", "DELETE")
            .Build();

        response.Links.Should().HaveCount(3);
    }
}
