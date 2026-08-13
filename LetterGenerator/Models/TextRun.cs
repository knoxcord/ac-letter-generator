using SkiaSharp;

namespace LetterGenerator.Models;

/// <summary>
/// A stretch of a line that draws as a unit: either literal text or a single emoji image
/// </summary>
/// <param name="Text">The source text, kept for emoji runs so they can be re-measured or fallen back to</param>
/// <param name="Emoji">The emoji artwork, or null when this run is literal text</param>
public readonly record struct TextRun(string Text, SKImage? Emoji)
{
    public bool IsEmoji => Emoji is not null;
}
