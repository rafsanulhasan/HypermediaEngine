using HypermediaEngine.Builders;

namespace HypermediaEngine.UnitTests.HypermediaEngine.Builders;

public sealed class HypermediaCollectionBuilderTests
{
    private record Product(int Id, string Name, decimal Price);

    [Test]
    public async Task Build_ShouldReturnResponseWithItems()
    {
        Product[] products = [new Product(1, "Widget", 9.99m), new Product(2, "Gadget", 19.99m)];
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(products);

        var response = builder.Build();

        using (Assert.Multiple())
        {
            await response.Items.Should().EqualTo(products);
            await response.TotalCount.Should().BeEqualTo(2);
        }
    }

    [Test]
    public async Task Build_ShouldHandleEmptyItems()
    {
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(new List<Product>());

        var response = builder.Build();
        using (Assert.Multiple())
        {
            await response.Items.Should().BeEmpty();
            await response.TotalCount.Should().BeEqualTo(0);
        }
    }

    [Test]
    public async Task Build_ShouldHandleNullItems()
    {
        var builder = new HypermediaCollectionBuilder<Product>().WithItems(new List<Product>());

        var response = builder.Build();

        await response.Items.Should().BeEmpty();
    }
}
