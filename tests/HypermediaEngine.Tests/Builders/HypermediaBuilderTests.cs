using Shouldly;

using NUnit.Framework;

using HypermediaEngine.Builders;
using HypermediaEngine.Responses;

using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Tests.Builders;

public sealed class HypermediaBuilderTests
{
    private record Product(int Id, string Name, decimal Price);

    [TestCase]
    public void Build_ShouldReturnResponseWithData()
    {
        Product product = new(1, "Widget", 9.99m);
        var builder = new HypermediaObjectBuilder<Product>().WithData(product);

        var response = builder.Build();

        response.Data.ShouldBe(product);
    }

    [TestCase]
    public void WithLink_ShouldOverwriteExistingLinkWithSameRel()
    {
        var builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m))
            .WithSelfLink("/products/1")
            .WithSelfLink("/products/1-updated");

        var response = builder.Build();

        Assert.Multiple(() =>
        {
            response.Links.ShouldNotBeNull();
            response.Links.Self.ShouldNotBeNull();
            response.Links.Self.Href.ShouldBe("/products/1-updated");
        });
    }

    [TestCase]
    public void WithLink_ShouldThrowForEmptyHref()
    {
        var builder = new HypermediaObjectBuilder<Product>();
        object act() => builder.WithSelfLink(string.Empty);
        Should.Throw<ArgumentNullException>(() => act());
    }

    [TestCase]
    public void WithMetadata_ShouldAddMetadataToResponse()
    {
        var builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m));

        var response = builder.Build();

        response.Meta.ShouldNotBeNull();
    }

    [TestCase]
    public void Build_ShouldReturnNullMetadataWhenNoneAdded()
    {
        var builder = new HypermediaObjectBuilder<Product>()
            .WithData(new Product(1, "Widget", 9.99m));

        var response = builder.Build();

        response.Meta.ShouldBeNull();
    }

    [TestCase]
    public void WithLink_ShouldSupportFluentChaining()
    {
        Product product = new(1, "Widget", 9.99m);
        var response = new HypermediaObjectBuilder<Product>()
            .WithData(product)
            .WithSelfLink("/products/1")
            .WithStateTransitionLink(LinkRelations.Update, "/products/1", HttpMethods.Put)
            .WithStateTransitionLink(LinkRelations.Delete, "/products/1", HttpMethods.Delete)
            .Build();

        Assert.Multiple(() =>
        {
            response.Links.ShouldNotBeNull();
            response.Links.Self.ShouldNotBeNull();
            response.Links.Self.Href.ShouldBe("/products/1");
            response.Links.StateTransitions.ShouldNotBeNull();
            response.Links.StateTransitions.ShouldContainKey(LinkRelations.Update);
            response.Links.StateTransitions[LinkRelations.Update].Href.ShouldBe("/products/1");
            response.Links.StateTransitions[LinkRelations.Update].Method.ShouldBe("PUT");
            response.Links.StateTransitions.ShouldContainKey(LinkRelations.Delete);
            response.Links.StateTransitions[LinkRelations.Delete].Href.ShouldBe("/products/1");
            response.Links.StateTransitions[LinkRelations.Delete].Method.ShouldBe("DELETE");
        });
    }
}
