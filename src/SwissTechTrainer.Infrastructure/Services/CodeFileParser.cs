using System.IO.Compression;
using System.Text;
using SwissTechTrainer.Application.Common.Interfaces;

namespace SwissTechTrainer.Infrastructure.Services;

/// <summary>
/// Parser service that extracts and formats single-file and multi-file zip archives for AI evaluation prompts.
/// </summary>
public class CodeFileParser : ICodeFileParser
{
    private static readonly HashSet<string> AllowedTextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".txt", ".md", ".json", ".xml", ".sql", ".yaml", ".yml", ".proto"
    };

    /// <inheritdoc />
    public async Task<string> ParseUploadedContentAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        string extension = Path.GetExtension(fileName);

        if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
        {
            return await ExtractZipArchiveAsync(fileStream, ct);
        }

        // Direct text file
        using var reader = new StreamReader(fileStream, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync(ct);
    }

    private static async Task<string> ExtractZipArchiveAsync(Stream zipStream, CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// ===========================================================");
        sb.AppendLine("// EXTRACTED ZIP ARCHIVE SUBMISSION");
        sb.AppendLine("// ===========================================================");

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
        int fileCount = 0;

        foreach (var entry in archive.Entries)
        {
            // Skip directory entries and bin/obj or git folders
            if (string.IsNullOrWhiteSpace(entry.Name) ||
                entry.FullName.Contains("/bin/", StringComparison.OrdinalIgnoreCase) ||
                entry.FullName.Contains("/obj/", StringComparison.OrdinalIgnoreCase) ||
                entry.FullName.Contains("/.git/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string entryExt = Path.GetExtension(entry.Name);
            if (!AllowedTextExtensions.Contains(entryExt))
            {
                continue;
            }

            fileCount++;
            sb.AppendLine();
            sb.AppendLine($"// -----------------------------------------------------------");
            sb.AppendLine($"// File: {entry.FullName}");
            sb.AppendLine($"// -----------------------------------------------------------");

            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8);
            string content = await reader.ReadToEndAsync(ct);
            sb.AppendLine(content);
        }

        if (fileCount == 0)
        {
            sb.AppendLine("// No readable .cs or text source files found inside the uploaded ZIP archive.");
        }

        return sb.ToString();
    }
}
