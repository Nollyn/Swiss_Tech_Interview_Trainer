namespace SwissTechTrainer.Application.Common.Interfaces;

public interface ICodeFileParser
{
    Task<string> ParseUploadedContentAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
