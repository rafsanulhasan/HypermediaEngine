using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Builders;
using HypermediaEngine.Responses;

using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Builders;

public sealed class HypermediaBuilderTests
{
    private record Product(int Id, string Name, decimal Price);

    [Test]
    public async Task Build_ShouldReturnResponseWithData()
    {
        Product product = new(1, "Widget", 9.99m);
        IHypermediaObjectBuilder<Product> builder = new HypermediaObjectBuilder<Product>().WithData(product);

        HypermediaObjectResponse<Product> response = builder.Build();

        await response.Data.Should().EqualTo(product);
    }

    [Test]
    public async Task WithLink_ShouldOverwriteExistingLinkWithSameRel()
    {
        IHypermediaObjectBuilder<Product> builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithSelfLink("/products/1")
            .WithSelfLink("/products/1-updated");

        HypermediaObjectResponse<Product> response = builder.Build();

        using (Assert.Multiple())
        {
            await response.Links.Should().NotBeNull();
            await response.Links!.Self.Should().NotBeNull();
            await response.Links!.Self.Href.Should().EqualTo("/products/1-updated");
        }
    }

    [Test]
    public void WithLink_ShouldThrowForEmptyHref()
    {
        var builder = new HypermediaObjectBuilder<Product>();
        void act() => builder.WithSelfLink(string.Empty);
        Assert.Throws<ArgumentException>(act);
    }

    [Test]
    public async Task WithMetadata_ShouldAddMetadataToResponse()
    {
        ObjectResponseMetadata metadata = new(EntityTag.Empty, "v1", 1);
        IHypermediaObjectBuilder<Product> builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithMetadata(metadata);

        HypermediaObjectResponse<Product> response = builder.Build();

        using (Assert.Multiple())
        {
            await response.Meta.Should().NotBeNull();
            await response.Meta.Should().EqualTo(metadata);
        }
    }

    [Test]
    public async Task Build_ShouldReturnNullMetadataWhenNoneAdded()
    {
        IHypermediaObjectBuilder<Product> builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m));

        HypermediaObjectResponse<Product> response = builder.Build();

        await response.Meta.Should().BeNull();
    }

    [Test]
    public async Task WithLink_ShouldSupportFluentChaining()
    {
        Product product = new(1, "Widget", 9.99m);
        HypermediaObjectResponse<Product> response = new HypermediaObjectBuilder<Product>()
            .WithData(product)
            .WithSelfLink("/products/1")
            .WithStateTransitionLink(LinkRelations.Update, "/products/1", HttpMethods.Put)
            .WithStateTransitionLink(LinkRelations.Delete, "/products/1", HttpMethods.Delete)
            .Build();

        using (Assert.Multiple())
        {
            await response.Data.Should().EqualTo(product);
            await response.Links.Should().NotBeNull();
            await response.Links!.Self.Should().NotBeNull();
            await response.Links!.Self.Href.Should().EqualTo("/products/1");
            await response.Links!.StateTransitions.Should().NotBeNull();
            await response.Links!.StateTransitions.Should().ContainKey(LinkRelations.Update);
            await response.Links!.StateTransitions![LinkRelations.Update].Href.Should().EqualTo("/products/1");
            await response.Links!.StateTransitions![LinkRelations.Update].Method.Should().EqualTo("PUT");
            await response.Links!.StateTransitions.Should().ContainKey(LinkRelations.Delete);
            await response.Links!.StateTransitions![LinkRelations.Delete].Href.Should().EqualTo("/products/1");
            await response.Links!.StateTransitions![LinkRelations.Delete].Method.Should().EqualTo("DELETE");
        }
    }
}
