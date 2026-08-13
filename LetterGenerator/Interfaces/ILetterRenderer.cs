using LetterGenerator.DTOs;
using LetterGenerator.Models;

namespace LetterGenerator.Interfaces;

public interface ILetterRenderer
{
    /// <summary>
    /// Renders the letter's fields onto an image and returns the encoded bytes along with the
    /// stationery they were drawn on.
    /// </summary>
    /// <param name="stationery">
    /// The stationery to draw on. When null one is chosen at random from those in season; when set, that
    /// stationery is used whether it is in season or not.
    /// </param>
    Task<RenderedLetter> RenderAsync(GenerateLetterRequest request, LetterType? stationery = null, CancellationToken cancellationToken = default);
}
