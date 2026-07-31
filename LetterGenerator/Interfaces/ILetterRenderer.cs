using LetterGenerator.DTOs;

namespace LetterGenerator.Interfaces;

public interface ILetterRenderer
{
    /// <summary>
    /// Renders the letter's fields onto an image and returns the encoded bytes.
    /// </summary>
    Task<byte[]> RenderAsync(GenerateLetterRequest request, CancellationToken cancellationToken = default);
}
