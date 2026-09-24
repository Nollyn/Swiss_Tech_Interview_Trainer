namespace SwissTechTrainer.Application.Common.Interfaces;

/// <summary>
/// Provides extraction and parsing capabilities for uploaded candidate solution files (.cs source files and multi-file .zip archives).
/// </summary>
public interface ICodeFileParser
{
    /// <summary>
    /// Parses and normalizes uploaded stream content into a structured source code representation.
    /// </summary>
    /// <param name="fileStream">The stream containing file bytes.</param>
    /// <param name="fileName">The original file name including extension.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Extracted source code text ready for evaluation.</returns>
    Task<string> ParseUploadedContentAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
