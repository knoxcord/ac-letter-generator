using System.Collections.Concurrent;
using System.Text;
using LetterGenerator.Interfaces;
using LetterGenerator.Models;
using SkiaSharp;

namespace LetterGenerator.Rendering;

/// <summary>
/// Serves Twemoji artwork from the bundled Emoji folder, indexed by the codepoint sequence in each file name
/// </summary>
public class EmojiCatalog : IEmojiCatalog, IDisposable
{
    private const string FileExtension = ".png";

    private const int ZeroWidthJoiner = 0x200D;
    private const int VariationSelector = 0xFE0F;

    private readonly string _directory;

    // Every codepoint sequence with artwork, and the first codepoint of each of those sequences.
    // Skia cannot combine an emoji sequence into one glyph on its own, so matching whole sequences against
    //   these names is what keeps a family or a skin tone from drawing as its separate pieces
    private readonly HashSet<string> _sequences;
    private readonly HashSet<int> _leadingCodepoints;

    // Longest sequence with artwork, which bounds how far ahead a match has to look
    private readonly int _longestSequence;

    // Decoded lazily and held for the life of the process, since a letter only ever touches a few emoji
    private readonly ConcurrentDictionary<string, SKImage?> _images = new();

    public EmojiCatalog()
    {
        _directory = Path.Combine(AppContext.BaseDirectory, "Emoji");

        if (!Directory.Exists(_directory))
        {
            throw new InvalidOperationException($"Emoji directory '{_directory}' does not exist.");
        }

        _sequences = [.. Directory
            .EnumerateFiles(_directory, $"*{FileExtension}")
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()];

        if (_sequences.Count == 0)
        {
            throw new InvalidOperationException($"Emoji directory '{_directory}' contains no {FileExtension} artwork.");
        }

        _leadingCodepoints = [.. _sequences.Select(sequence => ParseLeadingCodepoint(sequence))];
        _longestSequence = _sequences.Max(sequence => sequence.Count(character => character == '-') + 1);
    }

    public IEnumerable<TextRun> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        var runes = text.EnumerateRunes().ToArray();
        var literal = new StringBuilder();

        var index = 0;
        while (index < runes.Length)
        {
            var (sequence, length) = MatchLongestSequence(runes, index);
            var image = sequence is null ? null : GetImage(sequence);

            // Artwork that failed to decode falls back to the literal codepoints. That draws as tofu, but a
            //   letter with one bad glyph still beats failing the whole request
            if (image is null)
            {
                literal.Append(runes[index].ToString());
                index++;
                continue;
            }

            if (literal.Length > 0)
            {
                yield return new TextRun(literal.ToString(), null);
                literal.Clear();
            }

            yield return new TextRun(string.Concat(runes[index..(index + length)]), image);
            index += length;
        }

        if (literal.Length > 0)
        {
            yield return new TextRun(literal.ToString(), null);
        }
    }

    /// <summary>
    /// Finds the longest emoji sequence with artwork starting at <paramref name="start"/>.
    /// Longest wins so that a sequence is preferred over the shorter emoji it happens to begin with, which is
    /// what keeps 👍🏽 from matching the plain 👍 and leaving its skin tone to draw as a separate swatch.
    /// </summary>
    private (string? Sequence, int Length) MatchLongestSequence(Rune[] runes, int start)
    {
        // Ordinary prose never begins a sequence, so this check carries the overwhelming majority of positions
        if (!_leadingCodepoints.Contains(runes[start].Value))
        {
            return (null, 0);
        }

        var longestPossible = Math.Min(_longestSequence, runes.Length - start);

        for (var length = longestPossible; length >= 1; length--)
        {
            var sequence = BuildSequence(runes.AsSpan(start, length));

            if (_sequences.Contains(sequence))
            {
                return (sequence, length);
            }
        }

        return (null, 0);
    }

    /// <summary>
    /// Builds the file name Twemoji uses for <paramref name="runes"/>: lowercase hex codepoints joined by
    /// dashes, dropping <see cref="VariationSelector"/> unless the sequence is joined by <see cref="ZeroWidthJoiner"/>
    /// </summary>
    internal static string BuildSequence(ReadOnlySpan<Rune> runes)
    {
        var joined = false;

        foreach (var rune in runes)
        {
            if (rune.Value == ZeroWidthJoiner)
            {
                joined = true;
                break;
            }
        }

        var sequence = new StringBuilder();

        foreach (var rune in runes)
        {
            if (!joined && rune.Value == VariationSelector)
                continue;

            if (sequence.Length > 0)
                sequence.Append('-');

            sequence.Append(rune.Value.ToString("x"));
        }

        return sequence.ToString();
    }

    private static int ParseLeadingCodepoint(string sequence)
    {
        var end = sequence.IndexOf('-');
        var leading = end < 0 ? sequence : sequence[..end];

        return Convert.ToInt32(leading, 16);
    }

    private SKImage? GetImage(string sequence) => _images.GetOrAdd(sequence, key =>
    {
        try
        {
            return SKImage.FromEncodedData(Path.Combine(_directory, $"{key}{FileExtension}"));
        }
        catch (IOException)
        {
            return null;
        }
    });

    public void Dispose()
    {
        foreach (var image in _images.Values)
            image?.Dispose();

        _images.Clear();
        GC.SuppressFinalize(this);
    }
}
