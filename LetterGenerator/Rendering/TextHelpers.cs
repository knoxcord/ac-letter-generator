using System.Text;
using System.Text.RegularExpressions;
using LetterGenerator.Interfaces;
using SkiaSharp;

namespace LetterGenerator.Rendering;

public static partial class TextHelpers
{
    // Skia has no tracking setting, so we have to implement our own by drawing line one character at a time.
    private const float LetterSpacing = 3.0f;

    // Twemoji artwork is 72px square and gets drawn well below that, so resample rather than point sample
    private static readonly SKSamplingOptions EmojiSampling = new(SKCubicResampler.Mitchell);

    /// <summary>
    /// Rewrites Discord custom emoji markup into its plain <c>:name:</c> shortcode.
    /// Discord modals have no emoji picker, so these only arrive when an author pastes one in. There is no
    /// artwork for them here, and <c>:name:</c> reads as intended where the raw markup reads as a mistake.
    /// </summary>
    public static string RewriteCustomEmoji(string text) => CustomEmojiPattern().Replace(text, ":$1:");

    [GeneratedRegex(@"<a?:(\w{2,32}):\d+>")]
    private static partial Regex CustomEmojiPattern();

    /// <summary>
    /// Wrap words in <paramref name="text"/> into lines no wider than <paramref name="maxWidth"/>,
    /// stopping after <paramref name="maxLines"/>. Anything past that limit is dropped, since there is
    /// nowhere left on the card to put it. Line breaks in the text are kept as breaks to preserve formatting.
    /// </summary>
    public static List<string> WrapLines(string text, SKFont font, IEmojiCatalog emoji, float maxWidth, int maxLines)
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

            foreach (var word in SplitWordsLongerThanWidth(paragraph, font, emoji, maxWidth))
            {
                if (line.Length == 0)
                {
                    line.Append(word);
                    continue;
                }

                if (GetLineWidth($"{line} {word}", font, emoji) <= maxWidth)
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
    public static IEnumerable<string> SplitWordsLongerThanWidth(string text, SKFont font, IEmojiCatalog emoji, float maxWidth)
    {
        // Split on any whitespace
        foreach (var word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (GetLineWidth(word, font, emoji) <= maxWidth)
            {
                yield return word;
                continue;
            }

            var piece = new StringBuilder();
            foreach (var character in EnumerateDrawnUnits(word, emoji))
            {
                // Split words at the point where they reach max length
                if (piece.Length > 0 && GetLineWidth($"{piece}{character}", font, emoji) > maxWidth)
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
    public static void DrawSpacedText(SKCanvas canvas, string text, float x, float y, SKFont font, IEmojiCatalog emoji, SKPaint paint)
    {
        var currentX = x;

        foreach (var run in emoji.Tokenize(text))
        {
            if (run.Emoji is not null)
            {
                var size = GetEmojiSize(font);

                // Sit the artwork in the line box the glyphs occupy, so it lines up with the text either side
                canvas.DrawImage(run.Emoji, SKRect.Create(currentX, y + font.Metrics.Ascent, size, size), EmojiSampling);
                currentX += size + LetterSpacing;
                continue;
            }

            foreach (var rune in run.Text.EnumerateRunes())
            {
                var character = rune.ToString();
                canvas.DrawText(character, currentX, y, SKTextAlign.Left, font, paint);
                currentX += font.MeasureText(character) + LetterSpacing;
            }
        }
    }

    /// <summary>
    /// Calculates <paramref name="text"/> line width with <see cref="LetterSpacing"/> added between characters
    /// </summary>
    public static float GetLineWidth(string text, SKFont font, IEmojiCatalog emoji)
    {
        var width = 0.0f;

        foreach (var run in emoji.Tokenize(text))
        {
            if (run.Emoji is not null)
            {
                width += GetEmojiSize(font) + LetterSpacing;
                continue;
            }

            foreach (var rune in run.Text.EnumerateRunes())
            {
                width += font.MeasureText(rune.ToString()) + LetterSpacing;
            }
        }

        // The loop leaves a trailing gap after the last character that is not part of the text's width.
        return width > 0.0f ? width - LetterSpacing : 0.0f;
    }

    /// <summary>
    /// Calculates the total vertical space used by font glyphs
    /// </summary>
    // Ascent is negative (above baseline) and descent is positive (below baseline)
    public static float GetGlyphHeight(SKFontMetrics fontMetrics) => fontMetrics.Descent - fontMetrics.Ascent;

    /// <summary>
    /// Emoji are drawn square and as tall as the line box, which keeps them in proportion as
    /// <paramref name="font"/> is scaled down to fit its area
    /// </summary>
    private static float GetEmojiSize(SKFont font) => GetGlyphHeight(font.Metrics);

    /// <summary>
    /// Walks <paramref name="text"/> as the individual pieces it gets drawn in, where an emoji is one piece
    /// however many codepoints it spans. Splitting a word between two of these never severs an emoji.
    /// </summary>
    private static IEnumerable<string> EnumerateDrawnUnits(string text, IEmojiCatalog emoji)
    {
        foreach (var run in emoji.Tokenize(text))
        {
            if (run.Emoji is not null)
            {
                yield return run.Text;
                continue;
            }

            foreach (var rune in run.Text.EnumerateRunes())
            {
                yield return rune.ToString();
            }
        }
    }
}
