using FluentAssertions;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using UrlShortener.Api.Repository;

namespace UrlShortner.Tests;

public class UrlRepositoryTests
{
    [Fact]
    public void Load_Mappings_From_File()
    {
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "1 https://example.com abc");

        var repo = new FileUrlRepository(file, NullLogger<FileUrlRepository>.Instance);

        repo.TryGetLongUrl("abc", out var url).Should().BeTrue();
        url.Should().Be("https://example.com/");
    }

    [Fact]
    public void Returns_False_For_Missing_Code()
    {
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "");

        var repo = new FileUrlRepository(file, NullLogger<FileUrlRepository>.Instance);

        repo.TryGetLongUrl("missing", out _).Should().BeFalse();
    }
}