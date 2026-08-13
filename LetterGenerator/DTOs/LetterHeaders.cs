namespace LetterGenerator.DTOs;

public static class LetterHeaders
{
    /// <summary>
    /// Names the stationery a letter was drawn on. Send it back as
    /// <see cref="GenerateLetterRequest.Stationery"/> to draw on that same stationery again.
    /// </summary>
    public const string Stationery = "Letter-Stationery";
}
