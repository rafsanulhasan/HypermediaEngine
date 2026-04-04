namespace HypermediaEngine.Tests.Services;

using HypermediaEngine.Services;
using FluentAssertions;
using Xunit;

public class ETagServiceTests
{
    private readonly ETagService _service;

    public ETagServiceTests()
    {
        _service = new ETagService();
    }

    [Fact]
    public void GenerateETag_ShouldReturnNonEmptyString()
    {
        var resource = new { Id = 1, Name = "Test" };
        var etag = _service.GenerateETag(resource);
        etag.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateETag_ShouldReturnQuotedETag()
    {
        var resource = new { Id = 1, Name = "Test" };
        var etag = _service.GenerateETag(resource);
        etag.Should().StartWith("\"").And.EndWith("\"");
    }

    [Fact]
    public void GenerateETag_ShouldBeDeterministic()
    {
        var resource = new { Id = 1, Name = "Test" };
        var etag1 = _service.GenerateETag(resource);
        var etag2 = _service.GenerateETag(resource);
        etag1.Should().Be(etag2);
    }

    [Fact]
    public void GenerateETag_ShouldDifferForDifferentResources()
    {
        var resource1 = new { Id = 1, Name = "Test" };
        var resource2 = new { Id = 2, Name = "Other" };
        var etag1 = _service.GenerateETag(resource1);
        var etag2 = _service.GenerateETag(resource2);
        etag1.Should().NotBe(etag2);
    }

    [Fact]
    public void GenerateETag_ShouldThrowForNullResource()
    {
        var act = () => _service.GenerateETag(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsETagStale_ShouldReturnTrueWhenIfNoneMatchIsNull()
    {
        var result = _service.IsETagStale("\"abc\"", null);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsETagStale_ShouldReturnFalseWhenETagMatches()
    {
        var result = _service.IsETagStale("\"abc\"", "\"abc\"");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsETagStale_ShouldReturnTrueWhenETagDoesNotMatch()
    {
        var result = _service.IsETagStale("\"abc\"", "\"xyz\"");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsETagStale_ShouldBeCaseInsensitive()
    {
        var result = _service.IsETagStale("\"ABC\"", "\"abc\"");
        result.Should().BeFalse();
    }
}
