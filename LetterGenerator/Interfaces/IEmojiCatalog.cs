using LetterGenerator.Models;

namespace LetterGenerator.Interfaces;

/// <summary>
/// Supplies emoji artwork and recognises the emoji sequences within a piece of text
/// </summary>
public interface IEmojiCatalog
{
    /// <summary>
    /// Splits <paramref name="text"/> into runs of literal text and individual emoji.
    /// Text containing no emoji yields a single run.
    /// </summary>
    IEnumerable<TextRun> Tokenize(string text);
}
