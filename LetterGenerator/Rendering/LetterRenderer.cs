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
    private static readonly SKPoint TitlePoint = new(150.0f, 100.0f);
    private static readonly SKRect BodyArea = new(200.0f, 200.0f, 1030.0f, 580.0f);
    private static readonly SKPoint AuthorPoint = new(1100.0f, 700.0f);

    private const float BackgroundPaddingX = 20.0f;
    private const float BackgroundPaddingY = 15.0f;

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
        var (letterType, letter) = LetterTemplates.GetRandomLetter();

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
        var (lines, adjustedFont) = FitToBodyArea(text, bodyFont);

        if (lines.Count < 1)
            return;

        var metrics = adjustedFont.Metrics;
        var adjustedLineHeight = GetBodyLineHeight(adjustedFont);

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
        var widestLineWidth = lines.Max(line => TextHelpers.GetLineWidth(line, adjustedFont));
        var left = BodyArea.MidX - widestLineWidth / 2;

        if (paintBackground != null)
        {
            // Add the ascent and descent back in when calculating the rectangle here so that it spans the visible
            //   top and bottom edges of the lines of text
            // Ascent is negative here so adding it to firstBaseline expands rectangle's top edge _up_.
            // Descent is positive so adding it to the lastBaseline moves the rectangle's bottom edge _down_.
            var backgroundRectangle = new SKRect(left, firstBaseline + metrics.Ascent, left + widestLineWidth, lastBaseline + metrics.Descent);
            DrawBackground(canvas, backgroundRectangle, adjustedFont, paintBackground);
        }

        for (var line = 0; line < lines.Count; line++)
            TextHelpers.DrawSpacedText(canvas, lines[line], left, firstBaseline + line * adjustedLineHeight, adjustedFont, paint);
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
        var width = TextHelpers.GetLineWidth(text, font);
        var left = align switch
        {
            SKTextAlign.Center => point.X - width / 2,
            SKTextAlign.Right => point.X - width,
            _ => point.X,
        };

        var fontMetrics = font.Metrics;
        var backgroundRectangle = new SKRect(left, point.Y + fontMetrics.Ascent, left + width, point.Y + fontMetrics.Descent);
        DrawBackground(canvas, backgroundRectangle, font, paintBackground);
        TextHelpers.DrawSpacedText(canvas, text, left, point.Y, font, paintText);
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
    private static (List<string>, SKFont adjustedFont) FitToBodyArea(string text, SKFont font)
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
            var lines = TextHelpers.WrapLines(text, font, BodyArea.Width, lineLimit + 1);

            // The lines fit, so go ahead and return them as they are. Whatever the font size was left at is what
            //   the lines will be drawn at
            if (lines.Count <= lineLimit)
            {
                return (lines, font);
            }

            // If we cant reduce font size anymore, just cut all the lines after the lineLimit
            if (font.Size - BodyTextSizeStep < MinBodyTextSize)
            {
                lines.RemoveRange(lineLimit, lines.Count - lineLimit);
                return (lines, font);
            }

            // If we got here then text drawn with the current font size exceeded the line limit, so reduce font
            //   size and try again
            font.Size -= BodyTextSizeStep;
        }
    }
}
