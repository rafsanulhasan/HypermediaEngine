using HypermediaEngine.Abstractions;
using HypermediaEngine.Builders;
using HypermediaEngine.Responses;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Builders;

public sealed class HypermediaCollectionBuilderTests
{
    private record Product(int Id, string Name, decimal Price);

    [Test]
    public void Build_ShouldReturnResponseWithItems()
    {
        Product[] products = [new Product(1, "Widget", 9.99m), new Product(2, "Gadget", 19.99m)];
        IHypermediaCollectionBuilder<Product> builder = new HypermediaCollectionBuilder<Product>().WithItems(products);

        HypermediaCollectionResponse<Product> response = builder.Build();

        response.Items.ShouldBeEquivalentTo(products);
        response.TotalCount.ShouldBe(2);
    }

    [Test]
    public void Build_ShouldHandleEmptyItems()
    {
        IHypermediaCollectionBuilder<Product> builder = new HypermediaCollectionBuilder<Product>().WithItems(new List<Product>());

        HypermediaCollectionResponse<Product> response = builder.Build();

        response.Items.ShouldBeEmpty();
        response.TotalCount.ShouldBe(0);
    }

    [Test]
    public void Build_ShouldHandleNullItems()
    {
        IHypermediaCollectionBuilder<Product> builder = new HypermediaCollectionBuilder<Product>().WithItems(new List<Product>());

        HypermediaCollectionResponse<Product> response = builder.Build();

        response.Items.ShouldBeEmpty();
    }
}
