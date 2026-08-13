namespace LetterGenerator.Models;

/// <summary>
/// A drawn letter and the stationery it was drawn on
/// </summary>
/// <param name="Stationery">
/// Reported back so a caller can ask for the same stationery again, which is how a letter gets
/// redrawn with a different valediction without the design changing underneath it
/// </param>
/// <param name="Image">The encoded WebP bytes</param>
public sealed record RenderedLetter(LetterType Stationery, byte[] Image);
