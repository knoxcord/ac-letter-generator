using System.Text;
using SkiaSharp;

namespace LetterGenerator.Rendering;

public static class TextHelpers
{
    // Skia has no tracking setting, so we have to implement our own by drawing line one character at a time.
    private const float LetterSpacing = 3.0f;

    /// <summary>
    /// Wrap words in <paramref name="text"/> into lines no wider than <paramref name="maxWidth"/>,
    /// stopping after <paramref name="maxLines"/>. Anything past that limit is dropped, since there is
    /// nowhere left on the card to put it. Line breaks in the text are kept as breaks to preserve formatting.
    /// </summary>
    public static List<string> WrapLines(string text, SKFont font, float maxWidth, int maxLines)
    {
        var lines = new List<string>();

        if (maxLines <= 0 || string.IsNullOrWhiteSpace(text))
        {
            return lines;
        }

        // Normalize line endings and remove any preceding/trailing line breaks
        var paragraphs = text.ReplaceLineEndings("\n").Trim().Split('\n');
        var line = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            line.Clear();

            foreach (var word in SplitWordsLongerThanWidth(paragraph, font, maxWidth))
            {
                if (line.Length == 0)
                {
                    line.Append(word);
                    continue;
                }

                if (GetLineWidth($"{line} {word}", font) <= maxWidth)
                {
                    line.Append(' ').Append(word);
                    continue;
                }

                lines.Add(line.ToString());

                if (lines.Count == maxLines)
                    return lines;

                line.Clear().Append(word);
            }

            // Add whatever the paragraph ended on, which is just an empty string if the author left a blank line
            lines.Add(line.ToString());

            if (lines.Count == maxLines)
                return lines;
        }

        return lines;
    }

    /// <summary>
    /// Splits any word longer than <paramref name="maxWidth"/> into separate pieces.
    /// This prevents something like a url or long text without whitespace from flowing off the card
    /// </summary>
    public static IEnumerable<string> SplitWordsLongerThanWidth(string text, SKFont font, float maxWidth)
    {
        // Split on any whitespace
        foreach (var word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (GetLineWidth(word, font) <= maxWidth)
            {
                yield return word;
                continue;
            }

            var piece = new StringBuilder();
            foreach (var rune in word.EnumerateRunes())
            {
                var character = rune.ToString();

                // Split words at the point where they reach max length
                if (piece.Length > 0 && GetLineWidth($"{piece}{character}", font) > maxWidth)
                {
                    yield return piece.ToString();
                    piece.Clear();
                }

                piece.Append(character);
            }

            if (piece.Length > 0)
            {
                yield return piece.ToString();
            }
        }
    }

    /// <summary>
    /// Draws <paramref name="text"/> on the <paramref name="canvas"/> from a left-hand origin with <see cref="LetterSpacing"/> added between characters
    /// </summary>
    public static void DrawSpacedText(SKCanvas canvas, string text, float x, float y, SKFont font, SKPaint paint)
    {
        var currentX = x;

        foreach (var rune in text.EnumerateRunes())
        {
            var character = rune.ToString();
            canvas.DrawText(character, currentX, y, SKTextAlign.Left, font, paint);
            currentX += font.MeasureText(character) + LetterSpacing;
        }
    }

    /// <summary>
    /// Calculates <paramref name="text"/> line width with <see cref="LetterSpacing"/> added between characters
    /// </summary>
    public static float GetLineWidth(string text, SKFont font)
    {
        var width = 0.0f;

        foreach (var rune in text.EnumerateRunes())
        {
            width += font.MeasureText(rune.ToString()) + LetterSpacing;
        }

        // The loop leaves a trailing gap after the last character that is not part of the text's width.
        return width > 0.0f ? width - LetterSpacing : 0.0f;
    }
}