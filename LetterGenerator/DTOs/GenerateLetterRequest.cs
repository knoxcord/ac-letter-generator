namespace LetterGenerator.DTOs;

public class GenerateLetterRequest
{
    public required string Title { get; set; }

    public required string Body { get; set; }

    public required string Valediction { get; set; }
}
