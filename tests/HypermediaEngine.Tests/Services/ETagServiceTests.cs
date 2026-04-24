using EntityTagCaching.Services;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NUnit.Framework;

using Shouldly;

using System.Security.Cryptography;
using System.Text.Json;

namespace HypermediaEngine.Tests.Services;

public sealed class ETagServiceTests
{
    private readonly ETagService _service;

    public ETagServiceTests()
    {
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService<JsonSerializerOptions>().Returns(new JsonSerializerOptions());
        serviceProvider.GetService<ETagHasher>().Returns(SHA256.HashData);
        serviceProvider.GetService<ETagAsyncHasher>().Returns(SHA256.HashDataAsync);
        _service = new ETagService(serviceProvider);
    }

    [TestCase]
    public void GenerateETag_ShouldReturnNonEmptyString()
    {
        var resource = new { Id = 1, Name = "Test" };
        string etag = _service.GenerateETag(resource);
        etag.ShouldNotBeNullOrEmpty();
    }

    [TestCase]
    public void GenerateETag_ShouldReturnQuotedETag()
    {
        var resource = new { Id = 1, Name = "Test" };
        string etag = _service.GenerateETag(resource);
        Assert.Multiple(() =>
        {
            etag.ShouldStartWith("\"");
            etag.ShouldEndWith("\"");
        });
    }

    [TestCase]
    public void GenerateETag_ShouldBeDeterministic()
    {
        var resource = new { Id = 1, Name = "Test" };
        string etag1 = _service.GenerateETag(resource);
        string etag2 = _service.GenerateETag(resource);
        etag1.ShouldBe(etag2);
    }

    [TestCase]
    public void GenerateETag_ShouldDifferForDifferentResources()
    {
        var resource1 = new { Id = 1, Name = "Test" };
        var resource2 = new { Id = 2, Name = "Other" };
        string etag1 = _service.GenerateETag(resource1);
        string etag2 = _service.GenerateETag(resource2);
        etag1.ShouldNotBe(etag2);
    }

    [TestCase]
    public void GenerateETag_ShouldThrowForNullResource()
    {
        Func<object> act = () => _service.GenerateETag(null!);
        act.ShouldThrow<ArgumentNullException>();
    }

    [TestCase]
    public void IsETagStale_ShouldReturnTrueWhenIfNoneMatchIsNull()
    {
        bool result = _service.IsETagStale("\"abc\"", null);
        result.ShouldBeTrue();
    }

    [TestCase]
    public void IsETagStale_ShouldReturnFalseWhenETagMatches()
    {
        bool result = _service.IsETagStale("\"abc\"", "\"abc\"");
        result.ShouldBeFalse();
    }

    [TestCase]
    public void IsETagStale_ShouldReturnTrueWhenETagDoesNotMatch()
    {
        bool result = _service.IsETagStale("\"abc\"", "\"xyz\"");
        result.ShouldBeTrue();
    }

    [TestCase]
    public void IsETagStale_ShouldBeCaseInsensitive()
    {
        bool result = _service.IsETagStale("\"ABC\"", "\"abc\"");
        result.ShouldBeFalse();
    }
}
