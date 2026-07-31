using SkiaSharp;

namespace LetterGenerator.Models;

public class LetterTemplate(LetterTemplateOptions options)
{
    public readonly SKColor TitleColor = SKColor.Parse(options.TitleColor);
    public readonly SKColor BodyColor = SKColor.Parse(options.BodyColor ?? options.TitleColor);
    public readonly SKColor AuthorColor = SKColor.Parse(options.AuthorColor ?? options.TitleColor);
    public readonly SKColor? TextBackgroundColor = options.TextBackgroundColor is null ? null : SKColor.Parse(options.TextBackgroundColor);
    /// <summary>
    /// ints representing MMdd of start and end range
    /// </summary>
    public readonly (int Start, int End)? AvailableRange = options.AvailableRange;

};