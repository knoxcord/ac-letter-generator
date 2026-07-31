using System.Text;
using LetterGenerator.DTOs;
using LetterGenerator.Interfaces;
using LetterGenerator.Models;
using SkiaSharp;

namespace LetterGenerator.Rendering;

/// <summary>
/// Draws a letter onto a randomly chosen template with SkiaSharp.
/// </summary>
public class LetterRenderer(IStationarySource stationarySource) : ILetterRenderer
{
    // TODO: move this to DI
    private readonly LetterTemplates _letters = new();

    private static readonly SKPoint TitlePoint = new(150.0f, 100.0f);
    private static readonly SKRect BodyArea = new(200.0f, 200.0f, 1030.0f, 580.0f);
    private static readonly SKPoint AuthorPoint = new(1100.0f, 700.0f);

    private const float BackgroundPaddingX = 20.0f;
    private const float BackgroundPaddingY = 15.0f;

    // Skia has no tracking setting, so we have to implement our own by drawing line one character at a time.
    private const float LetterSpacing = 3.0f;

    // Body text scales to support larger amounts of text
    private const float TextSize = 40.0f;
    private const float MinBodyTextSize = 20.0f;
    private const float BodyTextSizeStep = 2.0f;

    // Reduce the line spacing of the font a bit to look more like AC
    private const float BodyLineHeightAdjustment = 0.75f;

    private readonly SKTypeface _bodyTypeface =
        SKTypeface.FromFile(Path.Combine(AppContext.BaseDirectory, "Fonts", "SeuratProB.otf"))
        ?? throw new InvalidOperationException(
            "Could not load Fonts/SeuratProB.otf from the output directory.");

    public async Task<byte[]> RenderAsync(GenerateLetterRequest request, CancellationToken cancellationToken = default)
    {
        var (letterType, letter) = _letters.GetRandomLetter();

        // Get the background image
        await using var stationary = await stationarySource.OpenStationary(letterType, cancellationToken);
        using var bitmap = SKBitmap.Decode(stationary)
            ?? throw new InvalidOperationException(
                $"Failed to decode the letter template image '{letterType}'.");

        // Setup the Skia canvas
        using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
        var canvas = surface.Canvas;
        canvas.DrawBitmap(bitmap, new SKPoint(0, 0), SKSamplingOptions.Default);

        // Shared by title and author text
        using var font = new SKFont { Typeface = _bodyTypeface, Size = TextSize };

        // Only used by body text since it can be scaled
        using var bodyFont = new SKFont { Typeface = _bodyTypeface, Size = TextSize };

        // Setup text colors
        using var paintTitle = new SKPaint { Color = letter.TitleColor, IsAntialias = true };
        using var paintBody = new SKPaint { Color = letter.BodyColor, IsAntialias = true };
        using var paintAuthor = new SKPaint { Color = letter.AuthorColor, IsAntialias = true };

        // Some backgrounds are busy enough to require a text background, so set that up here
        using SKPaint? paintBackground = letter.TextBackgroundColor.HasValue
            ? new SKPaint { Color = letter.TextBackgroundColor.Value, IsAntialias = true }
            : null;

        DrawText(canvas, request.Title, TitlePoint, SKTextAlign.Left, font, paintTitle, paintBackground);
        DrawBody(canvas, request.Body, bodyFont, paintBody, paintBackground);
        DrawText(canvas, request.Author, AuthorPoint, SKTextAlign.Right, font, paintAuthor, paintBackground);

        using var flatImage = surface.Snapshot();
        using var data = flatImage.Encode(SKEncodedImageFormat.Webp, 90);

        return data.ToArray();
    }

    /// <summary>
    /// Draw body <paramref name="text"/> on <paramref name="canvas"/> with adjusted font size and centering
    /// </summary>
    private static void DrawBody(SKCanvas canvas, string text, SKFont bodyFont, SKPaint paint, SKPaint? paintBackground)
    {
        // First adjust font and/or text length to ensure the lines will fit in the template body area
        var lines = FitToBodyArea(text, bodyFont);

        if (lines.Count < 1)
            return;

        var metrics = bodyFont.Metrics;
        var adjustedLineHeight = GetBodyLineHeight(bodyFont);

        // The distance between the top of the first line of text to the bottom of the last line of text.
        // If there is only one line then visible text height is just the height of the glyphs. If there are multiple
        //   lines then the baseline-to-baseline intervals between the lines are included as well
        var visibleTextHeight = (lines.Count - 1) * adjustedLineHeight + GetGlyphHeight(metrics);

        // Space before and after the visible text block
        // Calculated by finding the difference between the visible text height and the allowed body area, then
        //   divide by two to account for equal padding above and below the rendered text
        var padding = (BodyArea.Height - visibleTextHeight) / 2;

        // Ascent is negative, so subtract it to move down to the baseline of the first line of text
        var firstBaseline = BodyArea.Top + padding - metrics.Ascent;
        var lastBaseline = firstBaseline + (lines.Count - 1) * adjustedLineHeight;

        // Find the widest line so that we can build a single rectangular background rather than having each line
        //   render with a background only long enough to cover its text
        var widestLineWidth = lines.Max(line => GetLineWidth(line, bodyFont));
        var left = BodyArea.MidX - widestLineWidth / 2;

        if (paintBackground != null)
        {
            // Add the ascent and descent back in when calculating the rectangle here so that it spans the visible
            //   top and bottom edges of the lines of text
            // Ascent is negative here so adding it to firstBaseline expands rectangle's top edge _up_.
            // Descent is positive so adding it to the lastBaseline moves the rectangle's bottom edge _down_.
            var backgroundRectangle = new SKRect(left, firstBaseline + metrics.Ascent, left + widestLineWidth, lastBaseline + metrics.Descent);
            DrawBackground(canvas, backgroundRectangle, bodyFont, paintBackground);
        }

        for (var line = 0; line < lines.Count; line++)
            DrawSpacedText(lines[line], left, firstBaseline + line * adjustedLineHeight, canvas, bodyFont, paint);
    }

    /// <summary>
    /// Draw background on the <paramref name="canvas"/> at the given <paramref name="textBounds"/>
    /// </summary>
    private static void DrawBackground(SKCanvas canvas, SKRect textBounds, SKFont font, SKPaint? paintBackground)
    {
        if (paintBackground is null)
            return;

        var backgroundRectangle = textBounds;
        backgroundRectangle.Inflate(BackgroundPaddingX, BackgroundPaddingY);

        // Shape the background into a soft rounded pill
        var radius = GetGlyphHeight(font.Metrics) / 2 + BackgroundPaddingY;
        canvas.DrawRoundRect(backgroundRectangle, radius, radius, paintBackground);
    }

    /// <summary>
    /// Draw <paramref name="text"/> on the given <paramref name="canvas"/> at the supplied <paramref name="point"/>
    /// with background <paramref name="paintBackground"/> if defined
    /// </summary>
    private static void DrawText(SKCanvas canvas, string text, SKPoint point, SKTextAlign align, SKFont font, SKPaint paintText, SKPaint? paintBackground)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Handle alignment here because even though the text can be left, center, or right aligned,
        //   it is always rendered left-to-right.
        // This makes letter spacing easier and matches AC center align format where the paragraph
        //   is centered but the text is left aligned
        var width = GetLineWidth(text, font);
        var left = align switch
        {
            SKTextAlign.Center => point.X - width / 2,
            SKTextAlign.Right => point.X - width,
            _ => point.X,
        };

        var fontMetrics = font.Metrics;
        var backgroundRectangle = new SKRect(left, point.Y + fontMetrics.Ascent, left + width, point.Y + fontMetrics.Descent);
        DrawBackground(canvas, backgroundRectangle, font, paintBackground);
        DrawSpacedText(text, left, point.Y, canvas, font, paintText);
    }

    /// <summary>
    /// Get the baseline-to-baseline distance for <paramref name="font"/> lines, factoring in <see cref="BodyLineHeightAdjustment"/>
    /// </summary>
    private static float GetBodyLineHeight(SKFont font) => font.Spacing * BodyLineHeightAdjustment;

    /// <summary>
    /// Calculates the total vertical space used by font glyphs
    /// </summary>
    // Ascent is negative (above baseline) and descent is positive (below baseline)
    private static float GetGlyphHeight(SKFontMetrics fontMetrics) => fontMetrics.Descent - fontMetrics.Ascent;

    /// <summary>
    /// Wraps <paramref name="text"/> to the body area, stepping <paramref name="font"/> down a size at a
    /// time until every line fits. Text that still does not fit at <see cref="MinBodyTextSize"/> is cut off.
    /// </summary>
    // TODO: This is a load bearing mutation of the given font. We should return a copy instead of just quietly
    //   changing it
    private static List<string> FitToBodyArea(string text, SKFont font)
    {
        while (true)
        {
            // One line of text occupies a glyph height; each line after that adds a baseline interval on top.
            // So take the glyph height off the body area first, see how many intervals fit in the remainder,
            //   then add the glyph-only line back. This inverts the visibleTextHeight formula in DrawBody.
            var lineLimit = (int)((BodyArea.Height - GetGlyphHeight(font.Metrics)) / GetBodyLineHeight(font)) + 1;

            // WrapLines returns when lineLimit is met, so we need to add 1 to line limit in order to be able to
            //   tell whether the line limit was hit or not.
            // If we get a number of lines less than or equal to the lineLimit back, then we know it fits.
            // If we get a number of lines greater than the limit (i.e. lineLimit + 1), we know text was cut.
            var lines = WrapLines(text, font, BodyArea.Width, lineLimit + 1);

            // The lines fit, so go ahead and return them as they are. Whatever the font size was left at is what
            //   the lines will be drawn at
            if (lines.Count <= lineLimit)
            {
                return lines;
            }

            // If we cant reduce font size anymore, just cut all the lines after the lineLimit
            if (font.Size - BodyTextSizeStep < MinBodyTextSize)
            {
                lines.RemoveRange(lineLimit, lines.Count - lineLimit);
                return lines;
            }

            // If we got here then text drawn with the current font size exceeded the line limit, so reduce font
            //   size and try again
            font.Size -= BodyTextSizeStep;
        }
    }

    /// <summary>
    /// Wrap words in <paramref name="text"/> into lines no wider than <paramref name="maxWidth"/>,
    /// stopping after <paramref name="maxLines"/>. Anything past that limit is dropped, since there is
    /// nowhere left on the card to put it. Line breaks in the text are kept as breaks to preserve formatting.
    /// </summary>
    private static List<string> WrapLines(string text, SKFont font, float maxWidth, int maxLines)
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
    private static IEnumerable<string> SplitWordsLongerThanWidth(string text, SKFont font, float maxWidth)
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
    /// Draws <paramref name="text"/> from a left-hand origin with <see cref="LetterSpacing"/> added between characters
    /// </summary>
    private static void DrawSpacedText(string text, float x, float y, SKCanvas canvas, SKFont font, SKPaint paint)
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
    private static float GetLineWidth(string text, SKFont font)
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
