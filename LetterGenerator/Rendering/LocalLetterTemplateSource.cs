using LetterGenerator.Configuration;
using LetterGenerator.Interfaces;
using LetterGenerator.Models;
using Microsoft.Extensions.Options;

namespace LetterGenerator.Rendering;

/// <summary>
/// Serves templates from a directory on disk: the project's Images folder
/// </summary>
public class LocalStationarySource : IStationarySource
{
    private const string FileExtension = ".webp";
    private readonly string _directory;

    public LocalStationarySource(IOptions<LetterTemplateConfiguration> options, IWebHostEnvironment environment)
    {
        var configured = options.Value.DirectoryPath;
        _directory = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured);

        var fileNames = Enum.GetValues<LetterType>().Select(letterType => $"{letterType}{FileExtension}").ToList();

        // Verify at startup that all expected files are in place
        var missing = fileNames
            .Where(fileName => !File.Exists(Path.Combine(_directory, fileName)))
            .ToArray();

        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"Template directory '{_directory}' is missing {missing.Length} of " +
                $"{fileNames.Count} templates: {string.Join(", ", missing)}.");
        }
    }

    public Task<Stream> OpenStationary(LetterType letterType, CancellationToken cancellationToken = default)
    {
        var fileName = $"{letterType}{FileExtension}";
        var stream = File.OpenRead(Path.Combine(_directory, fileName));
        return Task.FromResult(stream as Stream);
    }
}
