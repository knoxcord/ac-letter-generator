using LetterGenerator.Models;

namespace LetterGenerator.Interfaces;

/// <summary>
/// Supplies letter templates
/// </summary>
public interface IStationarySource
{
    /// <summary>
    /// Opens the letter template associated with the letter type
    /// </summary>
    Task<Stream> OpenStationary(LetterType letterType, CancellationToken cancellationToken = default);
}
