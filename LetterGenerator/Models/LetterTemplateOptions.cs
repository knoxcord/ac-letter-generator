namespace LetterGenerator.Models;

public readonly record struct LetterTemplateOptions
{
    public string TitleColor { get; init; }
    public string? BodyColor { get; init; }
    public string? ValedictionColor { get; init; }
    public string? TextBackgroundColor { get; init; }
    /// <summary>
    /// ints representing MMdd of start and end range
    /// </summary>
    public (int Start, int End)? AvailableRange { get; init; }
}