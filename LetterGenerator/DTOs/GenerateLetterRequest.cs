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

    /// <summary>
    /// Stationery to keep out of the random pick, as named by the <see cref="LetterHeaders.Stationery"/>
    /// response header. Send back the one a letter was drawn on to reroll onto a different design.
    /// Ignored when <see cref="Stationery"/> names one to draw on.
    /// </summary>
    public LetterType? ExcludeStationery { get; set; }
}
