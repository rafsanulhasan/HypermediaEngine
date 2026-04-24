using HypermediaEngine.Builders;

using NUnit.Framework;

using Shouldly;

namespace HypermediaEngine.Tests.Builders;

public sealed class HypermediaCollectionBuilderTests
{
    private record Product(int Id, string Name, decimal Price);

    [TestCase]
    public void Build_ShouldReturnResponseWithItems()
    {
        Product[] products = [new Product(1, "Widget", 9.99m), new Product(2, "Gadget", 19.99m)];
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(products);

        var response = builder.Build();

        Assert.Multiple(() =>
        {
            response.Items.ShouldBeEquivalentTo(products);
            response.TotalCount.ShouldBe(2);
        });
    }

    [TestCase]
    public void Build_ShouldHandleEmptyItems()
    {
        var builder = new HypermediaCollectionBuilder<Product>().WithItems([]);

        var response = builder.Build();
        Assert.Multiple(() =>
        {
            response.Items.ShouldBeEmpty();
            response.TotalCount.ShouldBe(0);
        });
    }

    [TestCase]
    public void Build_ShouldHandleNullItems()
    {
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(null!);

        var response = builder.Build();

        response.Items.ShouldBeEmpty();
    }
}
