using LetterGenerator.Models;

namespace LetterGenerator.DTOs;

public class GenerateLetterRequest
{
    public required string Title { get; set; }

    public required string Body { get; set; }

    public required string Valediction { get; set; }

    /// <summary>
    /// The stationery to draw on, as named by the <see cref="LetterHeaders.Stationery"/> response
    /// header. Leave it out to have one picked at random from those in season.
    /// </summary>
    public LetterType? Stationery { get; set; }
}
