using LetterGenerator.Models;

namespace LetterGenerator.Interfaces;

/// <summary>
/// Supplies letter templates
/// </summary>
public interface IStationerySource
{
    /// <summary>
    /// Opens the letter template associated with the letter type
    /// </summary>
    Task<Stream> OpenStationery(LetterType letterType, CancellationToken cancellationToken = default);
}
