using System.IO.Compression;
using System.Text;
using FluentAssertions;
using SwissTechTrainer.Infrastructure.Services;
using Xunit;

namespace SwissTechTrainer.Infrastructure.Tests;

public class CodeFileParserTests
{
    private readonly CodeFileParser _sut = new();

    [Fact]
    public async Task ParseUploadedContentAsync_WithSingleCSharpFile_ReturnsFileContents()
    {
        string csharpCode = "public class OrderProcessor { public void Process() {} }";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csharpCode));

        string result = await _sut.ParseUploadedContentAsync(stream, "OrderProcessor.cs");

        result.Should().Be(csharpCode);
    }

    [Fact]
    public async Task ParseUploadedContentAsync_WithZipArchive_ExtractsAndAggregatesCodeFiles()
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry1 = archive.CreateEntry("Services/PaymentService.cs");
            using (var writer = new StreamWriter(entry1.Open()))
            {
                writer.Write("public class PaymentService {}");
            }

            var entry2 = archive.CreateEntry("Models/PaymentRequest.cs");
            using (var writer = new StreamWriter(entry2.Open()))
            {
                writer.Write("public record PaymentRequest;");
            }

            var ignoredBin = archive.CreateEntry("bin/Debug/test.dll");
            using (var writer = new StreamWriter(ignoredBin.Open()))
            {
                writer.Write("binary-junk");
            }
        }

        memoryStream.Position = 0;

        string result = await _sut.ParseUploadedContentAsync(memoryStream, "submission.zip");

        result.Should().Contain("Services/PaymentService.cs");
        result.Should().Contain("public class PaymentService {}");
        result.Should().Contain("Models/PaymentRequest.cs");
        result.Should().Contain("public record PaymentRequest;");
        result.Should().NotContain("binary-junk");
    }
}
