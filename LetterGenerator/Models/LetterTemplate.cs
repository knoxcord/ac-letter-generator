using SkiaSharp;

namespace LetterGenerator.Models;

public class LetterTemplate(LetterTemplateOptions options)
{
    public readonly SKColor TitleColor = SKColor.Parse(options.TitleColor);
    public readonly SKColor BodyColor = SKColor.Parse(options.BodyColor ?? options.TitleColor);
    public readonly SKColor ValedictionColor = SKColor.Parse(options.ValedictionColor ?? options.TitleColor);
    public readonly SKColor? TextBackgroundColor = options.TextBackgroundColor is null ? null : SKColor.Parse(options.TextBackgroundColor);
    /// <summary>
    /// ints representing MMdd of start and end range
    /// </summary>
    public readonly (int Start, int End)? AvailableRange = options.AvailableRange;

    private readonly string[] _keywords = options.Keywords ?? [];

    // Matches on a bare substring so that "birthdays" and "birthdayparty" both count. Keywords therefore
    //   need to be long enough that they cannot turn up inside an unrelated word
    public bool MatchesKeywords(string letterText) =>
        _keywords.Any(keyword => letterText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
};