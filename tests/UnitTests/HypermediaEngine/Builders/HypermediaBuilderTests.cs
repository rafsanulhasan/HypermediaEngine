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
    public void Build_ShouldReturnResponseWithData()
    {
        Product product = new(1, "Widget", 9.99m);
        IHypermediaObjectBuilder<Product> builder = new HypermediaObjectBuilder<Product>().WithData(product);

        HypermediaObjectResponse<Product> response = builder.Build();

        response.Data.ShouldBeEquivalentTo(product);
    }

    [Test]
    public void WithLink_ShouldOverwriteExistingLinkWithSameRel()
    {
        IHypermediaObjectBuilder<Product> builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithSelfLink("/products/1")
            .WithSelfLink("/products/1-updated");

        HypermediaObjectResponse<Product> response = builder.Build();

        response.Links.ShouldNotBeNull();
        response.Links!.Self.ShouldNotBeNull();
        response.Links!.Self.Href.ShouldBe("/products/1-updated");
    }

    [Test]
    public void WithLink_ShouldThrowForEmptyHref()
    {
        HypermediaObjectBuilder<Product> builder = new();
        void act() => builder.WithSelfLink(string.Empty);
        Should.Throw<ArgumentException>(act);
    }

    [Test]
    public void WithMetadata_ShouldAddMetadataToResponse()
    {
        ObjectResponseMetadata metadata = new(EntityTag.Empty, "v1", 1);
        IHypermediaObjectBuilder<Product> builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithMetadata(metadata);

        HypermediaObjectResponse<Product> response = builder.Build();

        response.Meta.ShouldNotBeNull();
        response.Meta.ShouldBeEquivalentTo(metadata);
    }

    [Test]
    public void Build_ShouldReturnNullMetadataWhenNoneAdded()
    {
        IHypermediaObjectBuilder<Product> builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m));

        HypermediaObjectResponse<Product> response = builder.Build();

        response.Meta.ShouldBeNull();
    }

    [Test]
    public void WithLink_ShouldSupportFluentChaining()
    {
        Product product = new(1, "Widget", 9.99m);
        HypermediaObjectResponse<Product> response = new HypermediaObjectBuilder<Product>()
            .WithData(product)
            .WithSelfLink("/products/1")
            .WithStateTransitionLink(LinkRelations.Update, "/products/1", HttpMethods.Put)
            .WithStateTransitionLink(LinkRelations.Delete, "/products/1", HttpMethods.Delete)
            .Build();

        response.Data.ShouldBeEquivalentTo(product);
        response.Links.ShouldNotBeNull();
        response.Links!.Self.ShouldNotBeNull();
        response.Links!.Self.Href.ShouldBe("/products/1");
        response.Links!.StateTransitions.ShouldNotBeNull();
        response.Links!.StateTransitions.ShouldContainKey(LinkRelations.Update);
        response.Links!.StateTransitions![LinkRelations.Update].Href.ShouldBe("/products/1");
        response.Links!.StateTransitions![LinkRelations.Update].Method.ShouldBe("PUT");
        response.Links!.StateTransitions.ShouldContainKey(LinkRelations.Delete);
        response.Links!.StateTransitions![LinkRelations.Delete].Href.ShouldBe("/products/1");
        response.Links!.StateTransitions![LinkRelations.Delete].Method.ShouldBe("DELETE");
    }
}
